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
//  01/31/2012  HGodinez           Created
// 

using System;
using System.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public static class HttpContextExtensions
    {
        public static void SetId(this HttpContext context, Guid id)
        {
            context.Items["loggingId"] = id;
        }

        public static Guid? GetId(this HttpContext context)
        {
            return context.Items.Contains("loggingId") ? (Guid) context.Items["loggingId"] : (Guid?) null;
        }

        public static HttpContext ToHttpContext(this HttpContextBase httpContextBase)
        {
            return httpContextBase.ApplicationInstance.Context;
        }

        public static string GetExternalId(this HttpContext context)
        {
            return context.Request.UserHostAddress;
        }
    }
}
