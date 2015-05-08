//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  12/13/2012  HGodinez           Created
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Engine.Globalization;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers
{
    public static class HttpContextExtensions
    {
        private static string _managementUrl;
        private static string ManagementUrl
        {
            get
            {
                return _managementUrl ?? (_managementUrl = Url.ToRelative(Url.ResolveTokens(Url.ManagementUrlToken + "/")));
            }
        }

        public static string GetManagementUrl(this HttpContext context)
        {
            return ManagementUrl;
        }

        public static bool TryGetSiteId(this HttpContext context, out string siteId)
        {
            siteId = GetN2SiteId(context);
            return !string.IsNullOrEmpty(siteId);
        }

        public static bool TryGetCultureCode(this HttpContext context, string siteId, out string cultureCode)
        {
            cultureCode = GetN2CultureCode(context, siteId);
            return !string.IsNullOrEmpty(cultureCode);
        }

        private static readonly string[] ClientIpSearchHeaders =
        {
            "HTTP_CLIENTIP", "CLIENTIP", "HTTP_X_ORIG_CLIENT_IP2",
            "HTTP_X_ORIG_CLIENT_IP", "X-ORIG-CLIENT-IP2", "X-ORIG-CLIENT-IP", "REMOTE_ADDR"
        };
        public static string GetClientIp(this HttpContext context)
        {
            var serverVariables = context.Request.ServerVariables;
            return ClientIpSearchHeaders.Select(clientIpSearchHeader => serverVariables[clientIpSearchHeader]).
                           FirstOrDefault(ip => !String.IsNullOrEmpty(ip));
        }

        private static string GetN2SiteId(HttpContext context)
        {
            var startPage = Find.StartPage as StartPage;
            if (startPage == null)
            {
                string siteId = null;
                var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
                if (routeData != null)
                {
                    var val = routeData.Values["siteId"];
                    siteId = val == null ? null : routeData.Values["siteId"].ToString();
                }

                var li = CmsFinder.FindLanguageIntersection();
                if (li != null)
                    startPage = li.GetChildren<StartPage>().FirstOrDefault(sp => sp.SiteID.Equals(siteId, StringComparison.InvariantCultureIgnoreCase));
            }

            return startPage == null ? null : startPage.SiteID;
        }

        private static string GetN2CultureCode(HttpContext context, string siteId)
        {
            var currentPage = Find.CurrentPage;
            if (currentPage == null)
                return GetN2LanguageFromUrl(context) ?? GetDefaultLanguageForSite(siteId);

            return GetN2LanguageFromCurrentPage(currentPage) ?? // First try to get the language from the current Page
                               GetN2LanguageFromUrl(context) ?? // Next try to get it from the URL, which should be able to override the user's browser culture code
                               GetN2LanguageFromUserAgent(currentPage, siteId, context.Request.UserLanguages); // Next try to get it from the user's browser culture code.
        }

        private static string GetDefaultLanguageForSite(string siteId)
        {
            var li = CmsFinder.FindLanguageIntersection();
            if (li != null)
            {
                var site = li.GetChildren<StartPage>()
                        .FirstOrDefault(sp => sp.SiteID.Equals(siteId, StringComparison.InvariantCultureIgnoreCase));
                if (site !=null) return site.LanguageCode;
            }
            return "en-US";
        }

        private static string GetN2LanguageFromCurrentPage(ContentItem currentPage)
        {
            var languageRoot = Find.ClosestOf<LanguageRoot>(currentPage) as ILanguage;
            return languageRoot == null ? null : languageRoot.LanguageCode;
        }

        private static string GetN2LanguageFromUrl(HttpContext context)
        {
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));

            if (routeData != null)
            {
                var cultureCode = routeData.Values["cultureCode"];
                return cultureCode == null ? null : cultureCode.ToString();
            }

            return null;
        }

        private static string GetN2LanguageFromUserAgent(ContentItem currentPage, string siteId, IEnumerable<string> userLanguages)
        {
            // TODO: If multiple sites have start pages that match of of the user's culture codes, is there some other way to fine-tune 
            // TODO: site selection instead of arbitrarily picking the first one?

            // BAD - This code won't work on XML Shopper nodes 
            try
            {
                var start = (Find.ClosestOf<StartPage>(currentPage) as StartPage);
                if (start == null)
                {
                    var p = currentPage.Children.FindNavigatablePages().OfType<StartPage>();
                    start = p.FirstOrDefault(x => x.SiteID.Equals(siteId, StringComparison.InvariantCultureIgnoreCase));
                }
                /*?? 
                    N2.Find.Items.Where.Type.Eq(typeof(StartPage))
                        .Select()
                        .Cast<StartPage>()
                        .FirstOrDefault(x => x.SiteID.Equals(siteId, StringComparison.InvariantCultureIgnoreCase));
                */

                if (start == null) return null;
                return CultureHelper.GetFirstMatchingLanguageRootOrDefault(start, userLanguages).LanguageCode;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}