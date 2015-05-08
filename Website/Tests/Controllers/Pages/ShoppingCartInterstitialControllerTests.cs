//using System.Collections.Generic;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Common;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Commerce.Web.Resources;
//using Moq;
//using N2.Web;
//using N2.Web.Mvc;
//using ViewModelBuilders.Catalog;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Pages
//{
//    public class ShoppingCartInterstitialControllerTests : TestBase
//    {
//        private readonly Mock<IProductViewModelBuilder> _prodViewModelBuilder = new Mock<IProductViewModelBuilder>();
//        private readonly Mock<IPageInfo> _pageInfo = new Mock<IPageInfo>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<IResourceService> _resourceService = new Mock<IResourceService>();
//        private readonly Mock<HttpContextBase> _httpContext = new Mock<HttpContextBase>();
       
//        public ShoppingCartInterstitialControllerTests()
//        {
//            DependencyRegistrar.
//                StandardDependencies()
//                               .With(_pageInfo.Object)
//                               .With(_prodViewModelBuilder.Object)
//                               .With(_resourceService.Object);
//            WebSession.Current = _webSession.Object;

//            _httpContext.Setup(c => c.Items).Returns(new Dictionary<string, object>());
//            _httpContext.Setup(c => c.Request).Returns(new HttpRequestWrapper(new HttpRequest("test", "http://localhost", null)));
//        }

//        [Fact]
//        public void Index_BasicViewModel()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            var res = ctrl.Index();
//            Assert.True(res is ViewResult);
//            Assert.True(((ViewResult)res).Model is ShoppingCartInterstitialViewModel);
//        }

//        [Fact]
//        public void Index_ProductId_FavorsUrlOverContentItem()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            ctrl.MyArguments = "123";
//            ctrl.MyCurrentPage.ProductID = "456";
//            string prodId = string.Empty; // sensing variable
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Callback<string>(pId =>
//                {
//                    prodId = pId;
//                });

//            ctrl.Index();

//            Assert.Equal("123", prodId);
//        }

//        [Fact]
//        public void Index_ProductId_UsesContentItemIfUrlAbsent()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            ctrl.MyArguments = "";
//            ctrl.MyCurrentPage.ProductID = "456";
//            string prodId = string.Empty; // sensing variable
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Callback<string>(pId =>
//                {
//                    prodId = pId;
//                });

//            ctrl.Index();

//            Assert.Equal("456", prodId);
//        }

//        [Fact]
//        public void Index_ModelProperties()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            ctrl.MyCurrentPage.ProductID = "456";
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail("456")).Returns(MakeProduct());

//            var mod = ((ViewResult)ctrl.Index()).Model as ShoppingCartInterstitialViewModel;

//            Assert.NotNull(mod.Product);
//            // check just a few fields to make sure controller slammed the product into the model
//            Assert.Equal("dpID", mod.Product.DisplayProduct.Id);
//            Assert.Equal("pID", mod.Product.Product.Id);
//        }

//        [Fact]
//        public void Index_SavesProductInWebSession()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            ctrl.MyCurrentPage.ProductID = "456";
//            ProductDetailPageViewModel prodInSlot = null;
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Returns(MakeProduct());
//            _webSession.Setup(w => w.Set(WebSession.CurrentProductSlot, It.IsAny<ProductDetailPageViewModel>()))
//                       .Callback<string, ProductDetailPageViewModel>((slot, prod) => { prodInSlot = prod; });

//            ctrl.Index();

//            Assert.NotNull(prodInSlot);
//            Assert.Equal("pID", prodInSlot.Product.Id);
//        }

//        [Fact]
//        public void Index_SetsPageInfo()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            ctrl.MyCurrentPage.ProductID = "456";
//            var pageName = "Service";
//            var pageNameSet = false;
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Returns(MakeProduct());
//            _pageInfo.SetupSet(p => p.PageName = It.IsAny<string>())
//                     .Callback<string>(val =>
//                         {
//                             pageName = val;
//                             pageNameSet = true;
//                         });

//            ctrl.Index();

//            Assert.True(pageNameSet);
//            Assert.Equal("Sales.Interstitial", pageName);
//        }

//        [Fact]
//        public void GetPageTitle_1stFromCurrentItem()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.MyCurrentPage.PageTitle = "item page title";

//            var res = ctrl.CallGetPageTitle(null);

//            Assert.Contains("item page title", res);
//        }

//        [Fact]
//        public void GetPageTitle_2ndFromModel()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;

//            var res = ctrl.CallGetPageTitle(new ProductDetailPageViewModel()
//            {
//                Product =
//                    new Product
//                    {
//                        Attributes = new[] { new ExtendedAttribute { Name = "Custom Title", Value = "custom title" } }
//                    }
//            });

//            Assert.Contains("custom title", res);
//        }

//        [Fact]
//        public void GetPageTitle_3rdFromResource()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartInterstitialController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;
//            FakeResources.AddString("PageTitle", "PAGETYPE_ShoppingCartInterstitial", "resource title");

//            var res = ctrl.CallGetPageTitle(null);

//            Assert.Contains("resource title", res);
//        }

//        #region internals

//        private ProductDetailPageViewModel MakeProduct()
//        {
//            return new ProductDetailPageViewModel()
//                {
//                    DisplayProduct = new DisplayProduct {Id = "dpID"},
//                    Product =
//                        new Product
//                            {
//                                Id = "pID",
//                                IsDisplayable = true,
//                                IsPurchasable = true,
//                                Inventory = new ProductInventory {InStock = true}
//                            },
//                };
//        }

//        private class MyShoppingCartInterstitialController : ShoppingCartInterstitialController
//        {
//            public MyShoppingCartInterstitialController(IPageInfo pageInfo,
//                                                  ICatalogAdapter catalogAdapter, IProductViewModelBuilder prodViewModelBuilder)
//                : base(pageInfo, catalogAdapter, prodViewModelBuilder, null)
//            {
//                Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage, string.Empty, string.Empty, MyArguments));
//            }

//            public string MyArguments;

//            public ShoppingCartInterstitialPage MyCurrentPage = new ShoppingCartInterstitialPage
//                {
//                    Parent = new StartPage()
//                };

//            public bool IsManagingReturn = false;
//            protected override bool IsManaging { get { return IsManagingReturn; } }

//            // promote for calling in tests
//            public string CallGetPageTitle(ProductDetailPageViewModel model)
//            {
//                return GetPageTitle(model);
//            }
//        }
//        #endregion
//    }
//}
