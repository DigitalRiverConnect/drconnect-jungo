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
//  12/22/2012  EHornbostel        Using API
// 

using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(ContentPage))]
    public class ContentPageController : ContentControllerBase<ContentPage>
    {
        // inject 
        private readonly IPageInfo _pageInfo;

        public ContentPageController(IRequestLogger logger, ILinkGenerator linkGenerator, IPageInfo pageInfo, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            _pageInfo = pageInfo;
        }

        public override ActionResult Index()
        {
            AssertProductsLoaded();
            if (Arguments.Length > 0)
            {
                return NotFound();
            }
            
            var vm = new ContentPageViewModel
            {
                PageType = CurrentItem.GetType()
            };


            if (CurrentItem != null)
            {
                SetPageTitleSimple();
                vm.Text = CurrentItem.Text;
            }

            SetPageInfo();
            SetPageMetaData(vm);
            return View("PageTemplates/ContentPage", vm);
        }

        protected void SetPageInfo()
        {
            if (CurrentItem is StoreHomePage)
                _pageInfo.PageName = "Sales.StoreHome";
            else if (CurrentItem is HelpPage)
                _pageInfo.PageName = "Service.Help";
            else if (CurrentItem is AboutPage)
                _pageInfo.PageName = "Service.About";
            else if (CurrentItem is ContactUsPage)
                _pageInfo.PageName = "Service.Contactus";
            else
                _pageInfo.PageName = "NA";
        }
    }
}
