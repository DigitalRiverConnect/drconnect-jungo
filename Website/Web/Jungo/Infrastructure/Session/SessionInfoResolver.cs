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
//  01/26/2012  HGodinez           Created
// 

using System;
using System.Collections.Generic;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Infrastructure.Config.Models;
using Newtonsoft.Json;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session
{
    public class SessionInfoResolver : ISessionInfoResolver
    {
        public string SiteId { get { return IsInitialized ? WebSession.Current.SiteId : null; } }

        public string CultureCode { get { return IsInitialized ? WebSession.Current.CultureCode : null; } }

        [JsonIgnore]
        public bool IsInitialized
        {
            get { return WebSession.IsInitialized; }
        }

        public string GetSiteMarketPlaceName()
        {
            if (!WebSession.IsInitialized)
                return String.Empty;
            SiteInfo siteInfo;
            if (!WebSession.Current.TryGetSiteInfo(out siteInfo))
                return String.Empty;
            return siteInfo.MarketPlaceName;
        }
    }
}
