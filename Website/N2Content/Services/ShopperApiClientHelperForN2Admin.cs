using System;
using System.Web;
using Jungo.Api;
using Jungo.Infrastructure;
using N2;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services
{
    public static class ShopperApiClientHelperForN2Admin
    {
        public static void AssureLimitedAuthentication(bool forCartManagement)
        {
            var shopperApiClient = Context.Current.Container.Resolve<IClient>();
            if (!String.IsNullOrEmpty(shopperApiClient.BearerToken)) return;
            shopperApiClient.SetApiKeyFromConfig();
            shopperApiClient.AuthenticateForLimitedPublicAsync(forCartManagement).Wait();
            HttpContext.Current.Items[Constants.ShopperApiClientHttpContextItemKey] = shopperApiClient;
        }
    }
}
