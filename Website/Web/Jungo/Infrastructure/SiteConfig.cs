//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: 2009/08/15 17:40:01 $
// Modified by: $Author: ALiu $
// Revision: $Revision: 1.1 $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  01/07/2013  ALiu           Created
//  

using System;
using System.Linq;
using Jungo.Infrastructure.Config.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    // todo: refactor: move these somewhere else

    [Serializable]
    public class SiteConfig
    {
        public SiteInfo[] SiteInfos { get; set; }

        public bool TryGetSiteInfo(SiteCultureInfo siteCultureInfo, out SiteInfo siteInfo)
        {
            siteInfo = null;
            SiteInfo[] siteInfos;
            if (!GetSiteInfos(siteCultureInfo.SiteId, out siteInfos))
                return false;

            if (!String.IsNullOrEmpty(siteCultureInfo.CountryId))
            {
                siteInfo = siteInfos.FirstOrDefault(si =>
                    si.Locale.Equals(siteCultureInfo.Locale, StringComparison.InvariantCultureIgnoreCase) &&
                    si.Country.Equals(siteCultureInfo.CountryId, StringComparison.InvariantCultureIgnoreCase));
            }
            else if (siteInfos.Any(si => !String.IsNullOrEmpty(si.MarketPlaceName)))
            {
                siteInfo = siteInfos.FirstOrDefault(si =>
                    si.Locale.Equals(siteCultureInfo.Locale, StringComparison.InvariantCultureIgnoreCase) &&
                    !String.IsNullOrEmpty(si.MarketPlaceName) &&
                    si.IsDefaultMarketPlace);
            }
            if (siteInfo == null)
            {
                siteInfo = siteInfos.FirstOrDefault(si =>
                    si.Locale.Equals(siteCultureInfo.Locale, StringComparison.InvariantCultureIgnoreCase));
            }
            return siteInfo != null;
        }

        public bool TryGetSiteInfo(string siteId, string locale, out SiteInfo siteInfo)
        {
            SiteInfo[] siteInfos;
            if (!GetSiteInfos(siteId, out siteInfos))
            {
                siteInfo = null;
                return false;
            }

            siteInfo = siteInfos.FirstOrDefault(si => si.Locale.Equals(locale, StringComparison.InvariantCultureIgnoreCase));

            if (siteInfo != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetSiteInfo(string siteId, out SiteInfo siteInfo)
        {
            SiteInfo[] siteInfos;
            if (!GetSiteInfos(siteId, out siteInfos))
            {
                siteInfo = null;
                return false;
            }

            siteInfo = siteInfos.FirstOrDefault(si => si.IsDefault);
            return true;
        }

        private bool GetSiteInfos(string siteId, out SiteInfo[] siteInfos)
        {
            if (SiteInfos == null)
            {
                siteInfos = null;
                return false;
            }
            siteInfos = SiteInfos.Where(s => s.SiteId == siteId).ToArray();
            return siteInfos.Length > 0;
        }
    }
}
