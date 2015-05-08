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
//  03/21/2013  BRichan        Created
// 

using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(LanguageIntersection))]
    public class LanguageIntersectionController : ContentControllerBase<LanguageIntersection>
    {
        public LanguageIntersectionController(IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            
        }

        public override ActionResult Index()
        {
            // detect language from browser settings
            if (!IsManaging)
            {
                var language = CurrentItem.SelectLanguage(Request.UserLanguages);
                if (language != null && language != CurrentItem)
                    return Redirect(language.Url);

                if (CurrentItem.RedirectUrl != CurrentItem.Url)
                    return Redirect(CurrentItem.RedirectUrl);
            }

            return new EmptyResult(); // cannot display logical page
        }

    }
}