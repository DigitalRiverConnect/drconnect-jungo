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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ActionFilters;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Resources;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using ViewModelBuilders.Cart;
using ViewModelBuilders.Layout;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(ShoppingCartPart))]
    public class ShoppingCartPartController : PartControllerBase<ShoppingCartPart>
    {
        public static readonly string ErrMsgUpdateLineItemFailed = "UpdateLineItem_Failed";
        public static readonly string ErrMsgAddCouponFailed = "AddCoupon_Failed";
        public static readonly string ErrMsgChangeVariationProductOutOfStock = "ChangeVariation_Product_OutOfStock";
        public static readonly string ErrMsgChangeVariationGenericProduct = "ChangeVariation_GenericProduct";
        public static readonly string ErrMsgChangeVariationFailed = "ChangeVariation_Failed";
        public static readonly string ErrMsgUpdateShippingFailed = "UpdateShipping_Failed";

        private readonly ICartApi _cartApi;
        private readonly IShoppingCartViewModelBuilder _shoppingCartViewModelBuilder;
        private readonly IOffersViewModelBuilder _offersViewModelBuilder;
        private readonly ILinkGenerator _linkGenerator;

        public ShoppingCartPartController(IRequestLogger logger, ICartApi cartApi, IShoppingCartViewModelBuilder shoppingCartViewModelBuilder,
            IOffersViewModelBuilder offersViewModelBuilder, ILinkGenerator linkGenerator)
            : base(logger)
        {
            _shoppingCartViewModelBuilder = shoppingCartViewModelBuilder;
            _cartApi = cartApi;
            _offersViewModelBuilder = offersViewModelBuilder;
            _linkGenerator = linkGenerator;
        }

        public override ActionResult Index()
        {
            var shoppingCartViewModel = WebSessionGet<ShoppingCartViewModel>(WebSession.ShoppingCartSlot);
            return PartialView("Index", shoppingCartViewModel);
        }

        public ActionResult Checkout()
        {
            SiteInfo si;
            if (!WebSession.Current.TryGetSiteInfo(out si)) return HttpNotFound();

            var redirectUri = _cartApi.GetWebCheckoutUrlAsync(_cartApi.GetCartAsync().Result, si.DrThemeId).Result;
            return Redirect(redirectUri.AbsoluteUri);
        }

        [HttpPost]
//        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(AddProductModel addProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int quantity;
                    if (String.IsNullOrEmpty(addProduct.Quantity) || !int.TryParse(addProduct.Quantity, out quantity))
                        quantity = 1;
                    int offerId;
                    if (String.IsNullOrEmpty(addProduct.OfferId) || !int.TryParse(addProduct.OfferId, out offerId))
                        offerId = 0;
                    var cart = _cartApi.AddProductToCartAsync(Convert.ToInt32(addProduct.ProductId), quantity, offerId).Result;
                    WebSession.Current.SetPersistentProperty(WebSession.ShoppingCartCount, cart.TotalItemsInCart.ToString(CultureInfo.InvariantCulture));
                    return PartialView("ShoppingCart", _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result);
                }
                catch (ShopperApiException exc)
                {
                    var model = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                    model.ErrorMessages = new[] { exc.ShopperApiError.Description };
                    return PartialView("ShoppingCart", model);
                }
                catch (Exception)
                {
                    var model = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                    model.ErrorMessages = new[] { Res.ShoppingCart_UpdateLineItemFailed };
                    return PartialView("ShoppingCart", model);
                }
            }
            var vm = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
            vm.ErrorMessages = InternalGetErrorMessages(ModelState).ToArray();
            return PartialView("ShoppingCart", vm);
        }

        [DynamicActionResult]
        [OutputCache(Duration = 0)]
        public ActionResult GetCart()
        {
            return PartialView("ShoppingCart", _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UpdateLineItem(UpdateLineItemModel updateModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var cart = updateModel.Quantity == 0
                        ? _cartApi.DeleteLineItemAsync(updateModel.LineItemId).Result
                        : _cartApi.UpdateLineItemQuantityAsync(updateModel.LineItemId, updateModel.Quantity).Result;
                    WebSession.Current.SetPersistentProperty(WebSession.ShoppingCartCount, cart.TotalItemsInCart.ToString(CultureInfo.InvariantCulture));
                    if (updateModel.Quantity == 0)
                        return Redirect(_linkGenerator.GenerateShoppingCartLink());
                    return PartialView("ShoppingCart", _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result);
                }
                catch (ShopperApiException exc)
                {
                    var model = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                    model.ErrorMessages = new[] {exc.ShopperApiError.Description};
                    return PartialView("ShoppingCart", model);
                }
                catch (Exception)
                {
                    var model = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                    model.ErrorMessages = new[] { Res.ShoppingCart_UpdateLineItemFailed };
                    return PartialView("ShoppingCart", model);
                }
            }
            var vm = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
            vm.ErrorMessages = InternalGetErrorMessages(ModelState).ToArray();
            return PartialView("ShoppingCart", vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AddCoupon(AddCouponModel addCoupon)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(addCoupon.CouponCode))
                {
                    try
                    {
                        var cart = _cartApi.ApplyCouponAsync(addCoupon.CouponCode).Result;
                        WebSession.Current.SetPersistentProperty(WebSession.ShoppingCartCount, cart.TotalItemsInCart.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (ShopperApiException exc)
                    {
                        var model = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                        model.ErrorMessages = new[] { exc.ShopperApiError.Description };
                        return PartialView("ShoppingCart", model);
                    }
                    catch (Exception)
                    {
                        var model = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                        model.ErrorMessages = new[] { Res.ShoppingCart_AddCouponFailed };
                        return PartialView("ShoppingCart", model);
                    }
                }
                return PartialView("ShoppingCart", _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result);
            }
            var vm = _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
            vm.ErrorMessages = InternalGetErrorMessages(ModelState).ToArray();
            return PartialView("ShoppingCart", vm);
        }

        // the shopping cart content changed, so get an updated cross sell for the new shopping cart
        [DynamicActionResult]
        [OutputCache(Duration = 0)]
        public ActionResult GetUpdatedCrossSell(string viewName, string promotionId, string emptyCartPromotionId, string title, int maxNumberOfProducts)
        {
            var cart = _cartApi.GetCartAsync().Result;
            CrossSellViewModel vm = null;
            var lineItem = (cart.LineItems == null || cart.LineItems.LineItem == null) ? null : cart.LineItems.LineItem.FirstOrDefault();
            if (lineItem != null)
                vm = _offersViewModelBuilder.GetCrossSellViewModelAsync(promotionId, lineItem.Product, _linkGenerator.GenerateShoppingCartLink()).Result;

            // If there are no driving products, then try the empty cart promotion, as the CrossSellPartController does
            if (vm == null || vm.Offers.Count == 0)
                vm = _offersViewModelBuilder.GetCrossSellViewModelAsync(!string.IsNullOrEmpty(emptyCartPromotionId) ? emptyCartPromotionId : promotionId, _linkGenerator.GenerateShoppingCartLink()).Result;

            OffersViewModelBuilder.RemoveCartItemsFromOffers(vm, cart);

            if (vm == null)
                vm = new CrossSellViewModel { Offers = new List<CrossSellOfferViewModel>() };

            vm.Title = title;
            vm.PromotionId = promotionId;
            vm.EmptyCartPromotionId = emptyCartPromotionId;
            vm.MaxNumberOfProducts = maxNumberOfProducts;

            return PartialView(viewName, vm);
        }

        protected virtual IEnumerable<string> InternalGetErrorMessages(ModelStateDictionary modelState)
        {
            var result = new List<string>();
            foreach (var errorKey in modelState.Keys.Where(key => modelState[key].Errors.Count > 0))
            {
                result.AddRange(modelState[errorKey].Errors.Select(error => error.ErrorMessage));
            }
            return result;
        }
    }
}
