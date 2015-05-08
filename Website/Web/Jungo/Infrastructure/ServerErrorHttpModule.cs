using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public class ServerErrorHttpModule : BaseHttpModule
    {
        // public for testing
        public const char ErrorSepChar = '\u0001';
        public const string ServerErrorCookieName = "ServerErrorInfo";

        public static string EncodeError(HttpContext context)
        {
            Guid? guid = context.GetId();
            return EncodeError(
                guid == null ? "" : guid.Value.ToString(),
                context.Request.Url.ToString(),
                context.Error.GetBaseException().Message,
                context.Error.GetBaseException().StackTrace);
        }

        public static string EncodeError(string requestId, string url, string message, string stackTrace)
        {
            var res = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(requestId + ErrorSepChar + url + ErrorSepChar + message + ErrorSepChar + stackTrace));
            return res;
        }

        public static void DecodeError(string encodedValue, out string requestId, out string url, out string excpMessage, out string stackTrace)
        {
            requestId = string.Empty;
            url = string.Empty;
            excpMessage = string.Empty;
            stackTrace = string.Empty;
            if (string.IsNullOrEmpty(encodedValue)) return;
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue)).Split(ErrorSepChar);
            if (parts.Length != 4) return;
            requestId = parts[0];
            url = parts[1];
            excpMessage = parts[2];
            stackTrace = parts[3];
        }

        #region Overrides of BaseHttpModule

        protected override ITraceLogger Logger
        {
            get { return Jungo.Infrastructure.DependencyResolver.Current.Get<ITraceLogger>(); }
        }

        protected override void OnError(HttpContext context, EventArgs e)
        {
            // this method has been known to be called a second time for the same request if conditions are just right (or wrong rather)
            //   such as if some handler somewhere in the chain clears the server error
            //   in which case AllErrors will have a second exception
            if (context.AllErrors != null && context.AllErrors.Length > 1) return;

            var exception = context.Error;
            var httpException = exception as HttpException;
            var httpErrorCode = httpException == null ? (int?)null : httpException.GetHttpCode();

            // Send user to 404 page.
            context.Response.Clear();
            context.Server.ClearError();
            try
            {
                context.Response.StatusCode = httpErrorCode ?? 500;
            }
            catch (Exception ex)
            {
                Logger.Info(ex, "Cannot set status.");
            }
            var routeData = new RouteData();
            routeData.Values["controller"] = "Errors";
            routeData.Values["action"] = httpErrorCode == 404 ? "Http404" : "Http500";
            routeData.Values["exception"] = exception;

            context.Response.TrySkipIisCustomErrors = true;
            IController errorsController = new ErrorsController(Jungo.Infrastructure.DependencyResolver.Current.Get<ILinkGenerator>());
            var wrapper = new HttpContextWrapper(context);
            var rc = new RequestContext(wrapper, routeData);
            errorsController.Execute(rc);
        }

        #endregion
    }
}
