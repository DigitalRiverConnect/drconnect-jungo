//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// 

using System;
using System.Globalization;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Details;
using N2.Engine.Globalization;
using N2.Integrity;
using N2.Web.UI;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Language Root",
    Description = "This is the root page for each language in a site.",
    IconUrl = "~/Content/img/icons/radial16.png")]
    [TabContainer(Defaults.Containers.Site, "Site", 1100)]
    [RestrictParents(typeof(StartPage))]
    //[RecursiveContainer("SiteContainer", 1000, RequiredPermission = Permission.Administer)]
    //[FieldSetContainer(Defaults.Containers.Site, "Site", 1000, ContainerName = "SiteContainer")]
    public class LanguageRoot : ContentPage, ILanguage
    {
        private static ISessionInfoResolver _sessionInfoResolver;

        private static ISessionInfoResolver SessionInfoResolver
        {
            get { return _sessionInfoResolver ?? (_sessionInfoResolver = Context.Current.Container.Resolve<ISessionInfoResolver>()); }
        }

        #region ILanguage Members

        public string FlagUrl
        {
            get
            {
                if (string.IsNullOrEmpty(LanguageCode))
                    return "";

                var parts = LanguageCode.Split('-');
                return N2.Web.Url.ResolveTokens(string.Format("~/N2/Resources/Img/Flags/{0}.png", parts[parts.Length - 1].ToLower()));
            }
        }

        public override string IconUrl
        {
            get
            {
                var flagUrl = FlagUrl;
                return string.IsNullOrEmpty(flagUrl) ? base.IconUrl : flagUrl;
            }
        }


        [EditableLanguagesDropDown(Title = "Languages", ContainerName = Defaults.Containers.Site)]
        public string LanguageCode
        {
            get { return ((string)GetDetail("LanguageCode") ?? ""); }
            set { SetDetail("LanguageCode", value, ""); }
        }

        public string LanguageTitle
        {
            get
            {
                if (string.IsNullOrEmpty(LanguageCode))
                    return "";

                var langCodeDisplayNameFromCulture = new CultureInfo(LanguageCode).DisplayName;
                string country = null;
                var mkt = SessionInfoResolver.GetSiteMarketPlaceName();
                if (!string.IsNullOrEmpty(mkt))
                    country = mkt;
                if (String.IsNullOrEmpty(country))
                {
                    var start = langCodeDisplayNameFromCulture.IndexOf("(", StringComparison.Ordinal) + 1;
                    var length = langCodeDisplayNameFromCulture.IndexOf(")", StringComparison.Ordinal) - start;
                    country = langCodeDisplayNameFromCulture.Substring(start, length);
                }
                var language = langCodeDisplayNameFromCulture.Substring(0, langCodeDisplayNameFromCulture.IndexOf("(", StringComparison.Ordinal) - 1).Trim();
                var regionName = country + " - " + language;
                return regionName;
                

                //return string.IsNullOrEmpty(LanguageCode) ? "" : new CultureInfo(LanguageCode).DisplayName;
            }
        }

        #endregion
    }
}
