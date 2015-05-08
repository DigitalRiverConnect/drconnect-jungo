using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;
using ViewModelBuilders.Layout;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(CrossSellPart))]
    public class CrossSellPartController : ContentControllerBase<CrossSellPart>
    {
        private readonly IOffersViewModelBuilder _offersViewModelBuilder;
        private readonly ICartApi _cartApi;
        private readonly ILinkGenerator _linkGenerator;

        public CrossSellPartController(IRequestLogger logger, IOffersViewModelBuilder offersViewModelBuilder, ICartApi cartApi, ILinkGenerator linkGenerator)
            : base(logger, null, null)
        {
            _offersViewModelBuilder = offersViewModelBuilder;
            _cartApi = cartApi;
            _linkGenerator = linkGenerator;
        }

        public override ActionResult Index()
        {
            CrossSellViewModel vm = null;
            var promotionId = CurrentItem.PromotionId;
            int? maxNumberOfProducts = 0;
            if (CurrentItem is CrossSellInterstitialPart)
            {
                var siteRoot = CmsFinder.FindSitePageFromSiteId(WebSession.Current.SiteId);
                if (siteRoot != null && !string.IsNullOrEmpty(siteRoot.PromotionId))
                    promotionId = siteRoot.PromotionId;
            }

            if (CurrentPage is ShoppingCartPage)
            {
                // We are on a shopping cart page. Try to build the cross-sell data with the items in the cart.
                string emptyCartPromotionId = null;
                var part = CurrentItem as CandyRackPart;
                if (part != null)
                {
                    emptyCartPromotionId = part.EmptyCartPromotionId;
                    maxNumberOfProducts = part.MaxNumberOfProducts;
                }

                //todo: Get the cart from session??
                var cartViewModel = WebSession.Current.Get<ShoppingCartViewModel>(WebSession.ShoppingCartSlot);
                var cart = cartViewModel == null ? _cartApi.GetCartAsync().Result : cartViewModel.Cart;
                if (cart.LineItems.LineItem != null)
                {
                    foreach (var li in cart.LineItems.LineItem)
                    {
                        if (li.Product != null)
                            vm = _offersViewModelBuilder.GetCrossSellViewModelAsync(promotionId, li.Product, _linkGenerator.GenerateShoppingCartLink()).Result;
                        if (vm != null)
                             break;
                    }
                }

                // If nothing is in the cart, or if no Offers were returned for this product, try to use the default offers.
                if (vm == null || vm.Offers == null || vm.Offers.Count == 0)
                    vm = _offersViewModelBuilder.GetCrossSellViewModelAsync(promotionId, _linkGenerator.GenerateShoppingCartLink()).Result;

                // Set the empty cart promotion Id, which is used by an Ajax call to the ShoppingCartPart.GetUpdatedCrossSell()
                if (vm != null)
                    vm.EmptyCartPromotionId = emptyCartPromotionId;

                OffersViewModelBuilder.RemoveCartItemsFromOffers(vm, cart);
            }
            else if (CurrentPage is ProductPage || CurrentPage is ShoppingCartInterstitialPage)
            {
                ProductDetailPageViewModel product;
                if (WebSession.Current.TryGet(WebSession.CurrentProductSlot, out product))
                {
                    if (product != null)
                    {
                        var crossSellProduct = product.Product;
                        var scip = CurrentPage as ShoppingCartInterstitialPage;
                        if (scip != null && scip.ProductID != null)
                        {
                            var scipPid = long.Parse(scip.ProductID);
                            crossSellProduct = product.Product.Variations.Product.FirstOrDefault(p => p.Id == scipPid);
                        }


                        vm = _offersViewModelBuilder.GetCrossSellViewModelAsync(promotionId, crossSellProduct, _linkGenerator.GenerateShoppingCartLink()).Result;
                        maxNumberOfProducts = 999;
                    }
                }
            }

            if (vm == null)
                vm = new CrossSellViewModel { Offers = new List<CrossSellOfferViewModel>() };

            vm.Name = CurrentItem.Name;
            vm.Title = CurrentItem.Title;
            vm.PromotionId = promotionId;
            vm.MaxNumberOfProducts = maxNumberOfProducts ?? 0;
            return PartialView(vm);
        }
    }
}
