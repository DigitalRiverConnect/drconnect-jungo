//using System;
//using System.Linq;
//using System.Web.Mvc;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Core.Services.Cache;
//using DigitalRiver.CloudLink.Core.Services.Configuration;
//using DigitalRiver.CloudLink.Core.Services.Logging;
//using Jungo.Models.ShopperApi.Common;
//using Moq;
//using N2.Web;
//using N2.Web.Mvc;
//using ViewModelBuilders.Catalog;
//using Xunit;
//using Product = DigitalRiver.CloudLink.Commerce.Api.Catalog.Product;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Pages
//{
//    public class CatalogControllerTests : TestBase
//    {
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly Mock<IProductViewModelBuilder> _prodViewModelBuilder = new Mock<IProductViewModelBuilder>();
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
//        private readonly Mock<IPageInfo> _pageInfo = new Mock<IPageInfo>();
//        private readonly Mock<ISessionAdapter> _sessionAdapter = new Mock<ISessionAdapter>();
//        private readonly Mock<IConfigurationService> _configurationService = new Mock<IConfigurationService>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<ICacheService> _cacheService = new Mock<ICacheService>();
//        private readonly Mock<ICache<CategoryViewModel>> _categoryCache = new Mock<ICache<CategoryViewModel>>();
//        private readonly Mock<ISessionInfoResolver> _sessionInfoResolver = new Mock<ISessionInfoResolver>();
//        private  readonly Mock<ICategoryViewModelBuilder> _catViewModelBuilder = new Mock<ICategoryViewModelBuilder>();

//        public CatalogControllerTests()
//        {
//            DependencyRegistrar.
//                StandardDependencies()
//                               .With(_linkGenerator.Object)
//                               .With(_pageInfo.Object)
//                               .With(_sessionAdapter.Object)
//                               .With(_prodViewModelBuilder.Object)
//                               .WithMetadata(_cacheService.Object, "CacheService")
//                               .With(_sessionInfoResolver.Object)
//                               .With(_catViewModelBuilder.Object)
//                               .With(_catalogAdapter.Object);
//            WebSession.Current = _webSession.Object;
//            ConfigurationService.Register(_configurationService.Object);
//            _cacheService.Setup(cs => cs.GetCache<CategoryViewModel>(It.IsAny<CacheConfig>())).Returns(_categoryCache.Object);
//        }

//        #region GetProduct

//        // test: GetProduct() returns PartialViewResult
//        [Fact]
//        public void GetProduct_PartialViewResult()
//        {
//            // setup
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Returns(new ProductDetailPageViewModel());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            var res = ctrl.GetProduct("123");

//            // sense
//            Assert.True(res is PartialViewResult);
//        }

//        // test: GetProduct() renders Catalog/Product
//        [Fact]
//        public void GetProduct_Render()
//        {
//            // setup
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Returns(new ProductDetailPageViewModel());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            var res = ctrl.GetProduct("123");

//            // sense
//            var vres = res as PartialViewResult;
//                // a different test assures we have a PartialViewResult; no need to retest this here
//            Assert.Equal("ProductPartials/Product", vres.ViewName);
//        }

//        // test: GetProduct returns a model of type Product
//        [Fact]
//        public void GetProduct_ModelType()
//        {
//            // setup
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>())).Returns(new ProductDetailPageViewModel());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            var vres = ctrl.GetProduct("123") as PartialViewResult;
//                // a different test assures we have a PartialViewResult; no need to retest this here

//            // sense
//            Assert.True(vres.Model is ProductDetailPageViewModel);
//        }

//        // test: GetProduct returns Product retrieved from catalog adapter
//        [Fact]
//        public void GetProduct_Product()
//        {
//            // setup
//            var prodToReturn = new ProductDetailPageViewModel()
//                {
//                    Product = new Product {Description = "my product descr", Title = "my product title"}
//                };
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail("123")).Returns(prodToReturn);
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            var vres = ctrl.GetProduct("123") as PartialViewResult;
//            var prodReturned = vres.Model as ProductDetailPageViewModel; // a different test assures model is Product

//            // sense
//            Assert.Equal(prodToReturn.Product.Description, prodReturned.Product.Description);
//            Assert.Equal(prodToReturn.Product.Title, prodReturned.Product.Title);
//        }

//        // test: GetProduct calls catalog adapter correctly
//        [Fact]
//        public void GetProduct_CatSvc()
//        {
//            // setup
//            string prodId = string.Empty; // sensing variable
//            _prodViewModelBuilder.Setup(c => c.GetProductDetail(It.IsAny<string>()))
//                           .Returns(new ProductDetailPageViewModel())
//                           .Callback<string>(pId =>
//                               {
//                                   prodId = pId;
//                               });
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            ctrl.GetProduct("123");

//            // sense
//            Assert.Equal("123", prodId);
//        }

//        // test: GetProduct gracefully handles Exception by returning null Product
//        [Fact(Skip = "move to CatalogAdapterTests")]
//        public void GetProduct_Exception()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetProduct(It.IsAny<string>())).Throws(new Exception("oops"));
//            DependencyRegistrar.WithFakeLogger();
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            var vres = ctrl.GetProduct("123") as PartialViewResult;
//            var mod = vres.Model as ProductDetailPageViewModel;

//            // sense
//            Assert.Null(mod);
//        }

//        // test: GetProduct logs Exception
//        [Fact(Skip = "move to CatalogAdapterTests")]
//        public void GetProduct_Logs_Exception()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetProduct(It.IsAny<string>())).Throws(new Exception("oops"));
//            DependencyRegistrar.WithFakeLogger();
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<CatalogController>();

//            // test
//            ctrl.GetProduct("123");

//            // sense
//            Assert.Equal(1, FakeLogger.Log.Count);
//            Assert.Contains("product detail failed", FakeLogger.Log[0].Format);
//            Assert.NotNull(FakeLogger.Log[0].Ex);
//            Assert.Contains("oops", FakeLogger.Log[0].Ex.Message);
//            Assert.True(FakeLogger.Log[0].Args.Any());
//        }
//        #endregion

//        #region NextProductResultsByCategory
//        // test: passes parsed json facets to catalog adapter
//        [Fact]
//        public void NextProdByCat_Facets()
//        {
//            FacetSearchField[] facets = null;
//            _catViewModelBuilder.Setup(
//                c => c.SearchProductByCategoryAsync(It.IsAny<long>(), It.IsAny<PagingOptions>()))
//                .Callback<string, SearchOptions, FacetSearchField[]>((cId, so, fcts) => facets = fcts);
//            Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>()
//                .NextProductResultsByCategory("vn", "catid", string.Empty, string.Empty, string.Empty, string.Empty,
//                    "[{\"attributeName\": \"an1\",\"constraintQueries\": [\"cq1\"]}]");
//            Assert.NotNull(facets);
//            Assert.Equal(1, facets.Length);
//            Assert.Equal("an1", facets[0].AttributeName);
//            Assert.Equal(1, facets[0].Constraints.Length);
//            Assert.Equal("cq1", facets[0].Constraints[0].ConstraintQuery);
//        }
//        #endregion

//        #region NextProductResultsByKeyword
//        // test: passes parsed json facets to catalog adapter
//        [Fact]
//        public void NextProdByKw_Facets()
//        {
//            FacetSearchField[] facets = null;
//            _catalogAdapter.Setup(
//                c => c.SearchProduct(It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<FacetSearchField[]>()))
//                .Callback<string, SearchOptions, FacetSearchField[]>((cId, so, fcts) => facets = fcts);
//            Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>()
//                .NextProductResultsByKeyword("vn", "kw", string.Empty, string.Empty, string.Empty, string.Empty,
//                    "[{\"attributeName\": \"an1\",\"constraintQueries\": [\"cq1\"]}]");
//            Assert.NotNull(facets);
//            Assert.Equal(1, facets.Length);
//            Assert.Equal("an1", facets[0].AttributeName);
//            Assert.Equal(1, facets[0].Constraints.Length);
//            Assert.Equal("cq1", facets[0].Constraints[0].ConstraintQuery);
//        }
//        #endregion

//        #region GetRedirectFromConfig
//        [Fact]
//        public void GetRedirectFromConfig_NoRedirectWithNoConfig()
//        {
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>();
//            ActionResult res = null;
//            Assert.False(ctrl.CallGetRedirectFromConfig("123", ref res));
//            Assert.Null(res);
//        }
//        #endregion

//        #region GetRedirectFromSite
//        [Fact]
//        public void GetRedirectFromSite_NoRedirect()
//        {
//        }
//        #endregion

//        #region GetPageTitle

//        [Fact]
//        public void GetPageTitle_FavorsCurrentItem()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>();
//            ctrl.MyCurrentPage.PageTitle = "item page title";
//            FakeResources.AddString("PageTitle", "Category", "category title");

//            var res = ctrl.CallGetPageTitle(new CatalogPageViewModel()
//            {
//                CategoryId = 1,
//                Title = "category name"
//            });

//            Assert.Contains("item page title", res);
//        }

//        [Fact]
//        public void GetPageTitle_FallsBackToCategoryResource()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;
//            FakeResources.AddString("PageTitle", "Category", "category title");

//            var res = ctrl.CallGetPageTitle(new CatalogPageViewModel()
//            {
//                CategoryId = 1,
//                Title = "category name"
//            });

//            Assert.Contains("category title", res);
//        }

//        [Fact]
//        public void GetPageTitle_FallsBackToPageTypeResource()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;
//            FakeResources.AddString("PageTitle", "Category", string.Empty);
//            FakeResources.AddString("PageTitle", "PAGETYPE_Catalog", "page type title");

//            var res = ctrl.CallGetPageTitle(new CatalogPageViewModel()
//            {
//                CategoryId = 1,
//                Title = "category name"
//            });

//            Assert.Contains("page type title", res);
//        }

//        [Fact]
//        public void GetPageTitle_SubstitutesCategoryName()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;
//            FakeResources.AddString("PageTitle", "Category", "category title with {categoryname}");

//            var res = ctrl.CallGetPageTitle(new CatalogPageViewModel()
//            {
//                CategoryId = 1,
//                Title = "category name"
//            });

//            Assert.Contains("category title with category name", res);
//        }
//        #endregion

//        #region JsonToFacets
//        // test: JsonToFacets returns null when passed empty string
//        [Fact]
//        public void JsonToFacets_Null()
//        {
//            var res =
//                Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>().CallJsonToFacets(string.Empty);
//            Assert.Null(res);
//        }

//        // test: JsonToFacets returns null when passed gorp
//        [Fact]
//        public void JsonToFacets_NullOnInvalidJson()
//        {
//            var res =
//                Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>()
//                    .CallJsonToFacets("{gorpoids found here - caution]");
//            Assert.Null(res);
//        }

//        // test: JsonToFacets returns null when passed json but not our json
//        [Fact]
//        public void JsonToFacets_NullOnWrongJson()
//        {
//            var res =
//                Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>()
//                    .CallJsonToFacets("[{\"pimple\":\"scab\",\"bungee\":[1,2,7]}]");
//            Assert.Null(res);
//        }

//        // test: JsonToFacets returns null when passed empty json
//        [Fact]
//        public void JsonToFacets_NullOnEmptyJson()
//        {
//            var res =
//                Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>()
//                    .CallJsonToFacets("[]");
//            Assert.Null(res);
//        }

//        // test: JsonToFacets returns facets when passed valid json
//        [Fact]
//        public void JsonToFacets_ValidJson()
//        {
//            var res =
//                Core.Services.Utils.DependencyResolver.Current.Get<MyCatalogController>()
//                    .CallJsonToFacets(
//                    "[{\"attributeName\": \"popcorn\",\"constraintQueries\": [\"buttered\",\"carmeled\",\"chicken\"]},"+
//                    "{\"attributeName\": \"peanuts\",\"constraintQueries\": [\"shelled\",\"boiled\",\"linus\"]}]");
//            Assert.NotNull(res);
//            Assert.Equal(2, res.Length);
//            Assert.Equal("popcorn", res[0].AttributeName);
//            Assert.Equal("peanuts", res[1].AttributeName);
//            Assert.Equal(3, res[0].Constraints.Length);
//            Assert.Equal(3, res[1].Constraints.Length);
//            Assert.Equal("buttered", res[0].Constraints[0].ConstraintQuery);
//            Assert.Equal("carmeled", res[0].Constraints[1].ConstraintQuery);
//            Assert.Equal("chicken", res[0].Constraints[2].ConstraintQuery);
//            Assert.Equal("shelled", res[1].Constraints[0].ConstraintQuery);
//            Assert.Equal("boiled", res[1].Constraints[1].ConstraintQuery);
//            Assert.Equal("linus", res[1].Constraints[2].ConstraintQuery);
//        }
//        #endregion

//        #region internals
//        private class MyCatalogController : CatalogController
//        {
//            public MyCatalogController(ILogger logger, ICatalogAdapter catalogAdapter,
//                ICategoryViewModelBuilder catViewModelBuilder, IProductViewModelBuilder prodViewModelBuilder,
//                ILinkGenerator linkGenerator, IPageInfo pageInfo)
//                : base(logger, catViewModelBuilder, prodViewModelBuilder, catalogAdapter, linkGenerator, pageInfo, null)
//            {
//                Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage));
//            }

//            public readonly CatalogPage MyCurrentPage = new CatalogPage();

//            // promote for calling in tests
//            public bool CallGetRedirect(string categoryId, out ActionResult redirectResult)
//            {
//                return GetRedirect(categoryId, out redirectResult);
//            }
//            public bool CallGetRedirectFromSite(string categoryId, ref ActionResult redirectResult)
//            {
//                return GetRedirectFromSite(categoryId, ref redirectResult);
//            }
//            public bool CallGetRedirectFromConfig(string categoryId, ref ActionResult redirectResult)
//            {
//                return GetRedirectFromConfig(categoryId, ref redirectResult);
//            }
//            public string CallGetPageTitle(CatalogPageViewModel model)
//            {
//                return GetPageTitle(model);
//            }
//            public FacetSearchField[] CallJsonToFacets(string facetsJson)
//            {
//                return JsonToFacets(facetsJson);
//            }
//        }
//        #endregion
//    }
//}
