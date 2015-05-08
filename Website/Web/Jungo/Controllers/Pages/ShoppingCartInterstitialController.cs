using System;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure.Config.Models;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using ViewModelBuilders.Catalog;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(ShoppingCartInterstitialPage))]
    public class ShoppingCartInterstitialController : ContentControllerBase<ShoppingCartInterstitialPage>
    {
        private readonly IPageInfo _pageInfo;
        private readonly IProductViewModelBuilder _prodViewModelBuilder;

        public ShoppingCartInterstitialController(IPageInfo pageInfo, IProductViewModelBuilder prodViewModelBuilder,
            ICatalogApi catalogApi) : base(catalogApi)
        {
            _pageInfo = pageInfo;
            _prodViewModelBuilder = prodViewModelBuilder;
        }

        public override ActionResult Index()
        {
            if (CurrentPage == null)
                throw new Exception();

            var startPage = CmsFinder.FindStartPageOf(CurrentPage) as StartPage;
            if (startPage == null)
                return NotFound();

            var productId = Arguments.Length > 0 ?
                Arguments[0] :
                (CurrentItem == null || string.IsNullOrEmpty(CurrentItem.ProductID) ? null : CurrentItem.ProductID.Split().First());

            if (string.IsNullOrEmpty(productId))
                return NotFound();
            
            AssertProductsLoaded(int.Parse(productId));

            ProductDetailPageViewModel product = null;
            SiteInfo si;
            WebSession.Current.TryGetSiteInfo(out si);
            Product p;
            if (Products.TryGetValue(Convert.ToInt32(productId), out p))
                product = _prodViewModelBuilder.GetProductDetail(p, si, false).Result;
            if (IsManaging && product == null)
                product = new ProductDetailPageViewModel(si)
                    {
                        DisplayProduct = new Product {Id = 123},
                        Product =
                            new Product
                                {
                                    Id = 123,
                                    DisplayableProduct = true,
                                    Purchasable = true,
                                    InventoryStatus = new InventoryStatus { ProductIsInStock = true }
                                },
                    };

            var model = new ShoppingCartInterstitialViewModel
                {
                    Product = product,
                };

            WebSession.Current.Set(WebSession.CurrentProductSlot, product);
            ProductController.SetPageInfo(_pageInfo, product, "Sales.Interstitial");
            SetPageTitle(GetPageTitle(model.Product));

            return View(model);
        }

        protected virtual string GetPageTitle(ProductDetailPageViewModel model)
        {
            var title = GetPageTitleFromCurrentItem();
            if (!string.IsNullOrEmpty(title))
                return title;
            title = model.Product.DisplayName;
            return !string.IsNullOrEmpty(title) ? title : GetPageTitleFromResource();
        }


    }
}
