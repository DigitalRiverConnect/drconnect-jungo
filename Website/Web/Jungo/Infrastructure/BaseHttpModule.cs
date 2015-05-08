//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
//  

using System;
using System.Threading.Tasks;
using System.Web;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public abstract class BaseHttpModule: IHttpModule
    {
        const string SkipServerVariable = "X_SKIP";

        #region Implementation of IHttpModule

        private readonly string _managementUrl;

        protected BaseHttpModule()
        {
            _managementUrl = Url.ToRelative(Url.ResolveTokens(Url.ManagementUrlToken + "/")).ToLowerInvariant();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += CreateEventHandler(OnBeginRequest);
            context.EndRequest += CreateEventHandler(OnEndRequest);
            context.AuthenticateRequest += CreateEventHandler(OnAuthenticateRequest);
            context.Error += CreateEventHandler(OnError);
            context.MapRequestHandler += CreateEventHandler(OnMapRequest);
            var asynchelper = new EventHandlerTaskAsyncHelper(OnBeginRequestAsync);
            context.AddOnBeginRequestAsync(asynchelper.BeginEventHandler, asynchelper.EndEventHandler);
        }

        protected virtual void OnAuthenticateRequest(HttpContext context, EventArgs e)
        {
        }

        protected virtual void OnBeginRequest(HttpContext context, EventArgs e)
        {
        }

        protected virtual void OnMapRequest(HttpContext context, EventArgs e)
        {
        }

        protected virtual void OnEndRequest(HttpContext context, EventArgs e)
        {
        }

        protected virtual void OnError(HttpContext context, EventArgs e)
        {
        }

        protected async virtual Task OnBeginRequestAsync(object sender, EventArgs e)
        {
        }

        public virtual void Dispose()
        {
        }

        #endregion

        protected abstract ITraceLogger Logger { get; }

        private EventHandler CreateEventHandler(Action<HttpContext, EventArgs> handler)
        {
            // Make sure to add "X_SKIP" to "Allowed Server Variables"

            return (sender, e) =>
                       {
                           string req;
                           var application = sender as HttpApplication;
                           if (application == null || !ShouldInvokeHandler(application, out req))
                               return;

                           Logger.Debug(handler.Target.GetType().Name + "." + handler.Method.Name + " " + req);

                           handler(application.Context, e);                              
                       };
        }

        protected bool ShouldInvokeHandler(HttpApplication application, out string req)
        {
            req = string.Empty;
            if ("1".Equals(application.Request.ServerVariables[SkipServerVariable]))
                return false;

            req = (application.Request.AppRelativeCurrentExecutionFilePath ?? "").ToLowerInvariant();

            // TODO magic values may cause trouble with configured content -> move to settings
            if (req.StartsWith(_managementUrl) || req.StartsWith("~/coteries/") || req.StartsWith("~/scripts/") || req.StartsWith("~/content/") || req.EndsWith(".axd"))
                return false; // skip handlers for speed

            return true;
        }
    }
}
