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
//  06/01/2012  ALiu           Created
//  

using System;
using System.Web.Mvc;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using Ninject;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ErrorLoggingAttribute : HandleErrorAttribute
    {
        #region Dependency Injection

        [Inject]
        public IRequestLogger Logger { get; set; }

        #endregion

        public override void OnException(ExceptionContext filterContext)
        {
            try
            {
                Logger.Error(filterContext.Exception, null);
            }
            catch (Exception e)
            {
                Logger.Warn(e, e.Message);
            }
            
            base.OnException(filterContext);
        }
    }
}
