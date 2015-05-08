//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
// 

using System;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public class SessionLogHttpModule : BaseHttpModule
    {
        private readonly ICrypto _cryptographicService;
        const string IpAddress = "_ipAddress_";

        public SessionLogHttpModule(ICrypto cryptographicService)
        {
            _cryptographicService = cryptographicService;
        }

        #region Overrides of BaseHttpModule

        protected override ITraceLogger Logger
        {
            get { return DependencyResolver.Current.Get<ITraceLogger>(); }
        }

        protected override void OnBeginRequest(HttpContext context, EventArgs e)
        {
            var id = Guid.NewGuid();
            context.SetId(id);
            SetMarketPlaceParameter(context);

            WebSession.BeginSession(context, _cryptographicService);

            if (WebSession.IsInitialized)
            {
                var webSession = WebSession.Current;

                var clientIp = context.GetClientIp();
                webSession.SetPersistentProperty(IpAddress, clientIp);

                var sessionToken = webSession.Get<SessionToken>(WebSession.SessionTokenSlot);
                Logger.Debug("Session " + sessionToken + " IP " + clientIp);

            }
        }

        protected override void OnEndRequest(HttpContext context, EventArgs e)
        {
            if (WebSession.IsInitialized)
            {
                Logger.Debug("End Session");
                WebSession.EndSession(context, _cryptographicService);
            }
        }

        protected override void OnError(HttpContext context, EventArgs e)
        {
            var id = context.GetId();

            if (id == null || context.Error == null) return;

            var exception = context.Error.GetBaseException();
            Logger.Debug(exception, "OnError");
        }

        #endregion

        private static void SetMarketPlaceParameter(HttpContext context)
        {
            var mktp = context.Request.QueryString[WebSession.MarketPlaceParameter];
            if (!String.IsNullOrEmpty(mktp))
                context.Items[WebSession.MarketPlaceParameter] = mktp;
        }

    }
}