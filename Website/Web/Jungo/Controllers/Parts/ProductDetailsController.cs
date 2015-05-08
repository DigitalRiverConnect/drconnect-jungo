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
//  03/21/2013  BRichan        Created
//  

using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(ProductDetailsPart))]
    public class ProductDetailsController : PartControllerBase<ProductDetailsPart>
    {
        public ProductDetailsController(IRequestLogger logger) : base(logger)
        {
        }

        public override ActionResult Index()
        {
            var model = WebSession.Current.Get<ProductDetailPageViewModel>(WebSession.CurrentProductSlot);
            return PartialView(model);
        }

        private static ProductDetailPageViewModel DemoProductDetailPageViewModel()
        {
            SiteInfo si;
            WebSession.Current.TryGetSiteInfo(out si);
            var ret = new ProductDetailPageViewModel(si)
            {

            };

            return ret;
        }
    }
}
