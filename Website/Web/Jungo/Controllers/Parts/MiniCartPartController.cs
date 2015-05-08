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
//  04/03/2013  RWilson           Created
// 

using System;
using System.Globalization;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using Jungo.Api;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
	[Controls(typeof(MiniCartPart))]
	public class MiniCartPartController : PartControllerBase<MiniCartPart>
	{
	    private readonly ICartApi _cartApi;
	    private readonly ILinkGenerator _linkGenerator;
        public MiniCartPartController(IRequestLogger logger, ICartApi cartApi, ILinkGenerator linkGenerator)
            : base(logger)
		{
            _cartApi = cartApi;
            _linkGenerator = linkGenerator;
		}

		public override ActionResult Index()
		{
            return PartialView("Index", GetViewModel());
		}

        [OutputCache(Duration = 0)]
        public ActionResult GetMiniCart()
        {
            return PartialView("MiniCart", GetViewModel());
        }

        private MiniCartViewModel GetViewModel()
        {
            var model = new MiniCartViewModel();
            var quantityProperty = WebSession.Current.GetPersistentProperty(WebSession.ShoppingCartCount);
            if (!String.IsNullOrEmpty(quantityProperty))
                model.Quantity = Convert.ToInt32(quantityProperty);
            else
            {
                var cart = _cartApi.GetCartAsync().Result;
                if (cart != null)
                    model.Quantity = cart.TotalItemsInCart;
                WebSession.Current.SetPersistentProperty(WebSession.ShoppingCartCount,
                    model.Quantity.ToString(CultureInfo.InvariantCulture));
            }
            SiteInfo siteInfo;
            if (WebSession.Current.TryGetSiteInfo(out siteInfo) && siteInfo.IsCartHandledBySite)
            {
                model.IsCartHandledBySite = true;
                model.CartLink = _linkGenerator.GenerateShoppingCartLink();
            }
            else
            {
                model.IsCartHandledBySite = false;
                model.CartLink = siteInfo.DrShoppingCartUrl;
            }
            return model;
        }
	}
}
