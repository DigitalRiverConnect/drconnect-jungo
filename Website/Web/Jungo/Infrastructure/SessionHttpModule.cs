using System;
using System.Threading.Tasks;
using System.Web;
using Jungo.Api;
using Jungo.Implementations.ShopperApiV1;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public class SessionHttpModule : BaseHttpModule
    {
        private readonly ICrypto _cryptographicService;
        private const string SessionCookieName = "JungoSession";

        public SessionHttpModule(ICrypto cryptographicService)
        {
            _cryptographicService = cryptographicService;
        }

        protected override ITraceLogger Logger
        {
            get { return DependencyResolver.Current.Get<ITraceLogger>(); }
        }

        protected async override Task OnBeginRequestAsync(object sender, EventArgs e)
        {
            string req;
            var application = sender as HttpApplication;
            if (application == null || !ShouldInvokeHandler(application, out req)) return;
            var context = application.Context;
            var client = DependencyResolver.Current.Get<IClient>();
            client.SetApiKeyFromConfig();
            var requestLogger = (IRequestLogger)context.Items[RequestLogger.ItemKey];
            var cookie = context.Request.Cookies.Get(SessionCookieName);
            if (cookie != null)
            {
                var decrypted = _cryptographicService.Decrypt(cookie.Value);
                var parts = decrypted.Split(',');
                requestLogger.SessionId = parts[0];
                client.BearerToken = parts[1];
                client.RefreshToken = parts[2];
                if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]))
                    client.SessionToken = parts[3];
            }
            if (string.IsNullOrEmpty(client.BearerToken) || string.IsNullOrEmpty(requestLogger.SessionId) || string.IsNullOrEmpty(client.SessionToken))
            {
                try
                {
                    await client.AuthenticateForLimitedPublicAsync(true).ConfigureAwait(false);
                    requestLogger.SessionId = Guid.NewGuid().ToString("n");
                }
                catch (Exception)
                {
                    //todo: something clever
                }
            }
            context.Items[Constants.ShopperApiClientHttpContextItemKey] = client;
            ShopperApiClient.AddClient(requestLogger.RequestId, client);
            // the following code is designed to handle one and only one case: where we are handling the very first request since re-booting the app
            var zeroGuid = new Guid();
            var dummy = ShopperApiClient.GetClient(zeroGuid);
            if (dummy == null)
                ShopperApiClient.AddClient(zeroGuid, client);
            else
                ShopperApiClient.RemoveClient(zeroGuid);
        }

        protected override void OnEndRequest(HttpContext context, EventArgs e)
        {
            var client = context.Items[Constants.ShopperApiClientHttpContextItemKey] as IClient;
            if (client == null) return;
            var requestLogger = (IRequestLogger)context.Items[RequestLogger.ItemKey];
            var cookieStr = string.Format("{0},{1},{2},{3}", requestLogger.SessionId, client.BearerToken, client.RefreshToken, client.SessionToken);
            var encrypted = _cryptographicService.Encrypt(cookieStr);
            context.Response.Cookies.Set(new HttpCookie(SessionCookieName, encrypted));
            ShopperApiClient.RemoveClient(requestLogger.RequestId);
        }
    }
}