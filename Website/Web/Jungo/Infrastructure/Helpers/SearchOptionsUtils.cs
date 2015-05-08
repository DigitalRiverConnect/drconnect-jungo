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

using Jungo.Models.ShopperApi.Common;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers
{
    public static class SearchOptionsUtils
    {
        public static PagingOptions GetPagingOptions(string pageNumber = null, string pageSize = null, string sortBy = null, string sortDir = null)
        {
            var pn = 1;
            if (!string.IsNullOrEmpty(pageNumber) && !int.TryParse(pageNumber, out pn))
                pn = 0;

            var ps = 1;
            if (!string.IsNullOrEmpty(pageSize) && !int.TryParse(pageSize, out ps))
                ps = 0;

            return new PagingOptions
                       {
                           Page = pn == 0 ? (int?)null : pn,
                           PageSize = ps == 0 ? (int?)null : ps,
                           Sort = string.IsNullOrEmpty(sortBy) || string.IsNullOrEmpty(sortDir) ? null : string.Format("{0}-{1}",sortBy.Trim(), sortDir.Trim())
                       };
        }
    }
}