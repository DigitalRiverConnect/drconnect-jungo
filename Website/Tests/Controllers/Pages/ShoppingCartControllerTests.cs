//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.IO;
//using System.Security.Principal;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;
//using DigitalRiver.CloudLink.Commerce.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.ShoppingCart;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
//using DigitalRiver.CloudLink.Core.Services.Logging;
//using Moq;
//using N2;
//using N2.Collections;
//using N2.Security;
//using ViewModelBuilders.Cart;
//using ViewModelBuilders.Catalog;
//using Xunit;
//using ShoppingCartViewModel = DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart.ShoppingCartViewModel;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Pages
//{
//    public class ShoppingCartControllerTests : TestBase
//    {
//        private readonly Mock<IShoppingCartAdapter> _shoppingCartAdapter = new Mock<IShoppingCartAdapter>();
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
//        private readonly Mock<IProductViewModelBuilder> _prodViewModelBuilder = new Mock<IProductViewModelBuilder>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly HttpContextTestData _httpContextTestData = new HttpContextTestData();
//        private readonly Mock<IContentFinder> _contentFinder = new Mock<IContentFinder>();
//        private readonly Mock<ISecurityManager> _securityManager = new Mock<ISecurityManager>();
//        private readonly Mock<HttpContextBase> _httpContext = new Mock<HttpContextBase>();
//        private readonly Mock<IShoppingCartViewModelBuilder> _shoppingCartViewModelBuilder = new Mock<IShoppingCartViewModelBuilder>();

//        public ShoppingCartControllerTests()
//        {
//            DependencyRegistrar
//                .StandardDependencies()
//                .With(_shoppingCartAdapter.Object)
//                .With(_httpContextTestData)
//                .With(_linkGenerator.Object)
//                .With(_shoppingCartViewModelBuilder.Object)
//                .With(_prodViewModelBuilder.Object);
//            WebSession.Current = _webSession.Object;
//            HttpContext.Current = new HttpContext(new HttpRequest("test", "http://localhost", null), new HttpResponse(new StreamWriter(new MemoryStream())));
//            CmsFinder.TheFinder = _contentFinder.Object;
//            _securityManager.Setup(s => s.IsAuthorized(It.IsAny<ContentItem>(), It.IsAny<IPrincipal>())).Returns(true);
//            AccessFilter.CurrentSecurityManager = () => _securityManager.Object;
//            _httpContext.Setup(c => c.Items).Returns(new Dictionary<string, object>());
//            _httpContext.Setup(c => c.Request).Returns(new HttpRequestWrapper(HttpContext.Current.Request));
//        }

//        #region Index without query parameters

//        // test: Index() returns correct ViewResult
//        [Fact]
//        public void Index_ViewResult()
//        {
//            // setup
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            _shoppingCartViewModelBuilder.Setup(s => s.GetShoppingCart()).Returns(new ShoppingCartViewModel());

//            // test
//            var vres = (ViewResult)ctrl.Index();

//            // sense
//            Assert.NotNull(vres);
//            Assert.Equal("PageTemplates/Default", vres.ViewName);
//        }

//        // test: Index() puts cart into websession
//        [Fact]
//        public void Index_CartInSession()
//        {
//            // setup
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            _shoppingCartViewModelBuilder.Setup(s => s.GetShoppingCart())
//                                .Returns(new ShoppingCartViewModel { Count = 5 });

//            // test
//            ctrl.Index();

//            // sense
//            Assert.Equal(WebSession.ShoppingCartSlot, ctrl.WebSessionSetName);
//            Assert.NotNull(ctrl.WebSessionSetValue);
//            Assert.True(ctrl.WebSessionSetValue is ShoppingCartViewModel);
//            Assert.Equal(5, ((ShoppingCartViewModel)ctrl.WebSessionSetValue).Count);
//        }

//        #endregion

//        #region Buy/AddToCart

//        // test: Buy calls InternalAddToCart, converting input into AddProductModel[] - one product id
//        [Fact]
//        public void BuyOne()
//        {
//            // setup
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(lg => lg.GenerateShoppingCartLink()).Returns("http://example.org/cart");
//            SetupTryAddProduct();

//            // test
//            ctrl.Buy("123", skipInterstitial: true);

//            // sense
//            Assert.NotNull(ctrl.AddProductModels);
//            Assert.Equal(1, ctrl.AddProductModels.Length);
//            Assert.Equal("123", ctrl.AddProductModels[0].ProductId);
//            Assert.Equal("1", ctrl.AddProductModels[0].Quantity);
//        }

//        // test: Buy calls InternalAddToCart, converting input into AddProductModel[] - two product ids, quantity specified
//        [Fact]
//        public void BuyTwo()
//        {
//            // setup
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(lg => lg.GenerateShoppingCartLink()).Returns("http://example.org/cart");
//            SetupTryAddProduct();

//            // test
//            ctrl.Buy("123,456", 2, skipInterstitial: true);

//            // sense
//            Assert.NotNull(ctrl.AddProductModels);
//            Assert.Equal(2, ctrl.AddProductModels.Length);
//            Assert.Equal("123", ctrl.AddProductModels[0].ProductId);
//            Assert.Equal("2", ctrl.AddProductModels[0].Quantity);
//            Assert.Equal("456", ctrl.AddProductModels[1].ProductId);
//            Assert.Equal("2", ctrl.AddProductModels[1].Quantity);
//        }

//        // test: AddToCart calls InternalAddToCart, converting json into AddProductModel[]- one
//        [Fact]
//        public void AddToCartOne()
//        {
//            // setup
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(lg => lg.GenerateShoppingCartLink()).Returns("http://example.org/cart");
//            SetupTryAddProduct();

//            // test
//            ctrl.AddToCart("[{\"ProductId\": \"myProdId\",\"Quantity\":1, \"OfferId\":\"myOfferId\"}]", skipInterstitial: true);

//            // sense
//            Assert.NotNull(ctrl.AddProductModels);
//            Assert.Equal(1, ctrl.AddProductModels.Length);
//            Assert.Equal("myProdId", ctrl.AddProductModels[0].ProductId);
//            Assert.Equal("1", ctrl.AddProductModels[0].Quantity);
//            Assert.Equal("myOfferId", ctrl.AddProductModels[0].OfferId);
//        }

//        private void SetupTryAddProduct()
//        {
//            var errors = new List<string>();
//            _shoppingCartAdapter.Setup(a =>
//                a.AddProductWithErrors(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out errors)).Returns(new ShoppingCart());
//        }

//        // test: AddToCart calls InternalAddToCart, converting json into AddProductModel[]- two
//        [Fact]
//        public void AddToCartTwo()
//        {
//            // setup
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(lg => lg.GenerateShoppingCartLink()).Returns("http://example.org/cart");
//            SetupTryAddProduct();

//            // test
//            ctrl.AddToCart(
//                "[{\"ProductId\": \"myProdId\",\"Quantity\":1, \"OfferId\":\"myOfferId\"},{\"ProductId\": \"myProdId2\",\"Quantity\":2, \"OfferId\":\"myOfferId2\"}]", skipInterstitial: true);

//            // sense
//            Assert.NotNull(ctrl.AddProductModels);
//            Assert.Equal(2, ctrl.AddProductModels.Length);
//            Assert.Equal("myProdId", ctrl.AddProductModels[0].ProductId);
//            Assert.Equal("1", ctrl.AddProductModels[0].Quantity);
//            Assert.Equal("myOfferId", ctrl.AddProductModels[0].OfferId);
//            Assert.Equal("myProdId2", ctrl.AddProductModels[1].ProductId);
//            Assert.Equal("2", ctrl.AddProductModels[1].Quantity);
//            Assert.Equal("myOfferId2", ctrl.AddProductModels[1].OfferId);
//        }

//        // test: AddToCart passes a spsCode through
//        [Fact]
//        public void AddToCartWithScsCode()
//        {
//            // setup
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(lg => lg.GenerateShoppingCartLink()).Returns("http://example.org/cart");
//            SetupTryAddProduct();

//            // test
//            var result = ctrl.AddToCart("[{\"ProductId\": \"myProdId\",\"Quantity\":1, \"OfferId\":\"myOfferId\"}]", true, 0, 3000) as RedirectResult;

//            // sense
//            Assert.NotNull(result);
//            Assert.Contains("scsCode=3000", result.Url);
//        }

//        // test: AddToCart() returns Index if errors
//        [Fact]
//        public void AddToCart_IndexWithErrors()
//        {
//            // setup
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
//            var errors = new List<string> {"double drat"};
//            _shoppingCartAdapter.Setup(a =>
//                a.AddProductWithErrors(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out errors)).Returns(new ShoppingCart());
//            _shoppingCartViewModelBuilder.Setup(s => s.GetShoppingCart())
//                .Returns(new ShoppingCartViewModel {ShoppingCart = new ShoppingCart()});

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] {new AddProductModel {ProductId = "myProdId", Quantity = "1"}});

//            // sense
//            Assert.True(ret is ViewResult);
//            var vres = ret as ViewResult;
//            Assert.Equal("PageTemplates/Default", vres.ViewName);
//            Assert.Equal(WebSession.ShoppingCartSlot, ctrl.WebSessionSetName);
//            Assert.NotNull(ctrl.WebSessionSetValue);
//            Assert.True(ctrl.WebSessionSetValue is ShoppingCartViewModel);
//            Assert.Equal(1, ((ShoppingCartViewModel)ctrl.WebSessionSetValue).ErrorMessages.Length);
//            Assert.Contains("double drat", ((ShoppingCartViewModel)ctrl.WebSessionSetValue).ErrorMessages[0]);
//        }

//        // test: AddToCart() redirects if no errors
//        [Fact]
//        public void AddToCart_RedirectsNoErrors()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            SetupTryAddProduct();
//            _linkGenerator.Setup(l => l.GenerateShoppingCartLink()).Returns("shopTilYouDrop");

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] { new AddProductModel { ProductId = "myProdId", Quantity = "3" } }, true, 0);

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("shopTilYouDrop", red.Url);
//        }

//        // test: AddToCart() handles multiple product ids
//        [Fact]
//        public void AddToCart_MultipleProductIds()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            var errors = new List<string>();
//            _shoppingCartAdapter.Setup(s => s.AddProductWithErrors(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out errors))
//                .Returns(new ShoppingCart { Count = 1 })
//                .Verifiable();
//            _linkGenerator.Setup(l => l.GenerateShoppingCartLink()).Returns("shoppingIsMyLife");

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[]
//            {
//                new AddProductModel {ProductId = "myProdId1", Quantity = "3"},
//                new AddProductModel {ProductId = "myProdId2", Quantity = "3"}
//            });

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            _shoppingCartAdapter.Verify(v => v.AddProductWithErrors(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(2));
//        }

//        [Fact]
//        public void AddToCart_Interstitial_IgnoreIfMoreThanOneProd()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink("myProdId")).Returns("interstitial");
//            _linkGenerator.Setup(l => l.GenerateShoppingCartLink()).Returns("shoppingcart");
//            SetupTryAddProduct();

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[]
//            {
//                new AddProductModel {ProductId = "myProdId1", Quantity = "3"},
//                new AddProductModel {ProductId = "myProdId2", Quantity = "3"}
//            });


//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("shoppingcart", red.Url);
//        }

//        [Fact]
//        public void AddToCart_Interstitial_IgnoreIfSkip()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink("myProdId")).Returns("interstitial");
//            _linkGenerator.Setup(l => l.GenerateShoppingCartLink()).Returns("shoppingcart");
//            SetupTryAddProduct();

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] { new AddProductModel { ProductId = "myProdId", Quantity = "3" } }, true);

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("shoppingcart", red.Url);
//        }

//        [Fact]
//        public void AddToCart_Interstitial_RedirectToCustom()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink("myProdId")).Returns("veniVidiVisa");
//            SetupTryAddProduct();

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] { new AddProductModel { ProductId = "myProdId", Quantity = "3" } });

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("veniVidiVisa", red.Url);
//        }

//        [Fact]
//        public void AddToCart_Interstitial_RedirectToGeneric()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink("myProdId")).Returns(string.Empty);
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink()).Returns("I-Like-Pizza-I-Got-A-Wallet");
//            _prodViewModelBuilder.Setup(c => c.GetPromotionByDrivingProduct(It.IsAny<string>(), It.IsAny<string>()))
//                              .Returns(new CrossSellViewModel {Offers = new List<OfferResult>() {new OfferResult()}});
//            _webSession.SetupGet(w => w.SiteId).Returns("mySite");
//            SetStartPage("mySite", "myPromo");
//            SetupTryAddProduct();

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] { new AddProductModel { ProductId = "myProdId", Quantity = "3" } });

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("I-Like-Pizza-I-Got-A-Wallet", red.Url);
//            Assert.True(red.Url.EndsWith("/myProdId"));
//        }

//        [Fact]
//        public void AddToCart_Interstitial_IgnoreGenericIfNoPromosByProduct()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink("myProdId")).Returns(string.Empty);
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink()).Returns("interstitial");
//            _linkGenerator.Setup(l => l.GenerateShoppingCartLink()).Returns("shoppingcart");
//            _webSession.SetupGet(w => w.SiteId).Returns("mySite");
//            SetStartPage("mySite", "myPromo");
//            SetupTryAddProduct();

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] { new AddProductModel { ProductId = "myProdId", Quantity = "3" } });

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("shoppingcart", red.Url);
//        }

//        [Fact]
//        public void AddToCart_Interstitial_IgnoreGenericIfNoPromoOnStartPage()
//        {
//            // setup
//            _httpContextTestData.Url = "http://MySite.org";
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartController>();
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink("myProdId")).Returns(string.Empty);
//            _linkGenerator.Setup(l => l.GenerateInterstitialLink()).Returns("interstitial");
//            _linkGenerator.Setup(l => l.GenerateShoppingCartLink()).Returns("shoppingcart");
//            _webSession.SetupGet(w => w.SiteId).Returns("mySite");
//            SetStartPage("mySite", string.Empty);
//            SetupTryAddProduct();

//            // test
//            var ret = ctrl.CallRealInternalAddToCart(new[] { new AddProductModel { ProductId = "myProdId", Quantity = "3" } });

//            // sense
//            Assert.True(ret is RedirectResult);
//            var red = ret as RedirectResult;
//            Assert.Contains("shoppingcart", red.Url);
//        }

//        #endregion

//        private void SetStartPage(string siteId, string promoId)
//        {
//            _contentFinder.Setup(c => c.FindAll<LanguageIntersection>(null, null))
//                          .Returns(new[]
//                              {
//                                  new LanguageIntersection
//                                      {
//                                          Children =
//                                              new ItemList {new StartPage {SiteID = siteId, PromotionId = promoId}}
//                                      }
//                              });
//        }
//    }

//    public sealed class MyShoppingCartController : ShoppingCartController
//    {
//        public MyShoppingCartController(IShoppingCartAdapter shoppingCartAdapter, IShoppingCartViewModelBuilder shoppingCartViewModelBuilder,
//            ILogger logger, ILinkGenerator linkGenerator, IPageInfo pageInfo, HttpContextTestData myTestData, IProductViewModelBuilder prodViewModelBuilder)
//            : base(shoppingCartAdapter, logger, linkGenerator, pageInfo, prodViewModelBuilder, shoppingCartViewModelBuilder, null)
//        {
//            SessionId = "123";
//            CurrentItem = new ShoppingCartPage();
//            this.SetMockControllerContext(myTestData);
//        }

//        public string WebSessionSetName { get; private set; }
//        public object WebSessionSetValue { get; private set; }
//        protected override void WebSessionSet<TS>(string name, TS value)
//        {
//            WebSessionSetName = name;
//            WebSessionSetValue = value;
//        }

//        protected override TS WebSessionGet<TS>(string name)
//        {
//            return (TS)WebSessionSetValue;
//        }

//        public ActionResult CallRealInternalAddToCart(AddProductModel[] addProductModels, bool skipInterstitial = false, int cpeCode = 0)
//        {
//            //return base.InternalAddToCart(addProductModels, skipInterstitial, cpeCode);
//            return null;
//        }

//        public AddProductModel[] AddProductModels { get; private set; }

//        protected override Task<ActionResult> InternalAddToCart(AddProductModel[] addProductModels, bool skipInterstitial = false, int cpeCode = 0, int scsCode = 0)
//        {
//            AddProductModels = addProductModels;
//            //return base.InternalAddToCart(addProductModels, skipInterstitial, cpeCode, scsCode);
//            return null;
//        }
//    }
//}
