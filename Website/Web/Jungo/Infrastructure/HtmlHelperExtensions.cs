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
//  05/11/2012  HGodinez           Created
// 

using System.Web.Mvc;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public static class HtmlHelperExtensions
    {
        public static string GetRequestId(this HtmlHelper html)
        {
            var guid = html.ViewContext.HttpContext.ToHttpContext().GetId();
            return guid == null ? "" : guid.Value.ToString();
        }
    }
}
