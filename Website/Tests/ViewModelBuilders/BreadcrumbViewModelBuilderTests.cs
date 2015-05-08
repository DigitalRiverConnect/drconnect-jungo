//using System.Collections.Generic;
//using System.Linq;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout;
//using DigitalRiver.CloudLink.Core.Services.Cache;
//using Moq;
//using ViewModelBuilders.Layout;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.ViewModelBuilders
//{
//    public class BreadcrumbViewModelBuilderTests : TestBase
//    {
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly Mock<ICacheService> _cacheService = new Mock<ICacheService>();
//        private readonly Mock<ICacheKeyResolver> _cacheKeyResolver = new Mock<ICacheKeyResolver>();
//        private readonly Mock<ICache<IEnumerable<Breadcrumb>>> _breadcrumbCache = new Mock<ICache<IEnumerable<Breadcrumb>>>();
//        private readonly Mock<ISessionInfoResolver> _sessionInfoResolver = new Mock<ISessionInfoResolver>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();

//        public BreadcrumbViewModelBuilderTests()
//        {
//            DependencyRegistrar
//                 .StandardDependencies()
//                 .With(_linkGenerator.Object)
//                 .With(_catalogAdapter.Object)
//                 .With(_cacheKeyResolver.Object)
//                 .WithMetadata(_cacheService.Object, "CacheService")
//                 .WithSingleton<IBreadcrumbViewModelBuilder, BreadcrumbViewModelBuilder>()
//                 .With(_sessionInfoResolver.Object);

//            WebSession.Current = _webSession.Object;
//            _cacheService.Setup(cs => cs.GetCache<IEnumerable<Breadcrumb>>(It.IsAny<CacheConfig>())).Returns(_breadcrumbCache.Object);
//        }

//        #region GetBreadcrumbsFromCategory
//        // test: GetBreadcrumbsFromCategory returns already-cached breadcrumbs
//        [Fact]
//        public void GetBreadcrumbsFromCategory_GetsCached()
//        {
//            // setup
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            var bcs = new List<Breadcrumb> { new Breadcrumb { Title = "BC1", Url = "Url1" }, new Breadcrumb { Title = "BC2", Url = "Url2" } };
//            var res = bcs.AsEnumerable(); // want TryGet to set out param to this
//            _breadcrumbCache.Setup(b => b.TryGet(cacheKey, out res)).Returns(true);

//            // test
//            var ret = Core.Services.Utils.DependencyResolver.Current.Get<IBreadcrumbViewModelBuilder>().GetBreadcrumbsFromCategory("");

//            // sense
//            var arr = ret.ToArray();
//            Assert.Equal(2, arr.Length);
//            Assert.Equal("BC1", arr[0].Title);
//            Assert.Equal("Url1", arr[0].Url);
//            Assert.Equal("BC2", arr[1].Title);
//            Assert.Equal("Url2", arr[1].Url);
//        }

//        // test: GetBreadcrumbsFromCategory returns breadcrumbs after caching
//        [Fact]
//        public void GetBreadcrumbsFromCategory_Caches()
//        {
//            // setup
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            IEnumerable<Breadcrumb> res;
//            _breadcrumbCache.Setup(b => b.TryGet(cacheKey, out res)).Returns(false);
//            _catalogAdapter.Setup(s => s.GetBreadcrumbByCategoryId("123"))
//                           .Returns(new[]
//                               {
//                                   new ProductCategory {DisplayName = "BC1", CategoryId = "1"},
//                                   new ProductCategory {DisplayName = "BC2", CategoryId = "2"}
//                               });
//            _linkGenerator.Setup(l => l.GenerateCategoryLink("1", false)).Returns("Url1");
//            _linkGenerator.Setup(l => l.GenerateCategoryLink("2", false)).Returns("Url2");
//            IEnumerable<Breadcrumb> cachedCrumbs = null;
//            _breadcrumbCache.Setup(b => b.Add(cacheKey, It.IsAny<IEnumerable<Breadcrumb>>()))
//                            .Callback<CacheKey, IEnumerable<Breadcrumb>>((c, b) =>
//                            { cachedCrumbs = b; });

//            // test
//            var ret = Core.Services.Utils.DependencyResolver.Current.Get<IBreadcrumbViewModelBuilder>().GetBreadcrumbsFromCategory("123");

//            // sense
//            var arr = ret.ToArray();
//            Assert.Equal(2, arr.Length);
//            Assert.Equal("BC1", arr[0].Title);
//            Assert.Equal("Url1", arr[0].Url);
//            Assert.Equal("BC2", arr[1].Title);
//            Assert.Equal("Url2", arr[1].Url);
//            var cache = cachedCrumbs.ToArray();
//            Assert.NotNull(cache);
//            Assert.Equal(2, cache.Length);
//        }

//        // test: GetBreadcrumbsFromCategory caches empty breadcrumbs if no breadcrumbs for category id
//        [Fact]
//        public void GetBreadcrumbsFromCategory_Empty()
//        {
//            // setup
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            IEnumerable<Breadcrumb> res;
//            _breadcrumbCache.Setup(b => b.TryGet(cacheKey, out res)).Returns(false);
//            _catalogAdapter.Setup(s => s.GetBreadcrumbByCategoryId("123")).Returns((ProductCategory[])null);
//            IEnumerable<Breadcrumb> cachedCrumbs = null;
//            _breadcrumbCache.Setup(b => b.Add(cacheKey, It.IsAny<IEnumerable<Breadcrumb>>()))
//                            .Callback<CacheKey, IEnumerable<Breadcrumb>>((c, b) =>
//                            { cachedCrumbs = b; });

//            // test
//            var ret = Core.Services.Utils.DependencyResolver.Current.Get<IBreadcrumbViewModelBuilder>().GetBreadcrumbsFromCategory("123");

//            // sense
//            var arr = ret.ToArray();
//            Assert.Equal(0, arr.Length);
//            var cache = cachedCrumbs.ToArray();
//            Assert.NotNull(cache);
//            Assert.Equal(0, cache.Length);
//        }
//        #endregion

//        #region GetBreadcrumbsFromProduct
//        // test: GetBreadcrumbsFromProduct returns already-cached breadcrumbs
//        [Fact]
//        public void GetBreadcrumbsFromProduct_GetsCached()
//        {
//            // setup
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            var bcs = new List<Breadcrumb> { new Breadcrumb { Title = "BC1", Url = "Url1" }, new Breadcrumb { Title = "BC2", Url = "Url2" } };
//            var res = bcs.AsEnumerable(); // want TryGet to set out param to this
//            _breadcrumbCache.Setup(b => b.TryGet(cacheKey, out res)).Returns(true);

//            // test
//            var ret = Core.Services.Utils.DependencyResolver.Current.Get<IBreadcrumbViewModelBuilder>().GetBreadcrumbsFromProduct("");

//            // sense
//            var arr = ret.ToArray();
//            Assert.Equal(2, arr.Length);
//            Assert.Equal("BC1", arr[0].Title);
//            Assert.Equal("Url1", arr[0].Url);
//            Assert.Equal("BC2", arr[1].Title);
//            Assert.Equal("Url2", arr[1].Url);
//        }

//        // test: GetBreadcrumbsFromProduct returns breadcrumbs after caching
//        [Fact]
//        public void GetBreadcrumbsFromProduct_Caches()
//        {
//            // setup
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            IEnumerable<Breadcrumb> res;
//            _breadcrumbCache.Setup(b => b.TryGet(cacheKey, out res)).Returns(false);
//            _catalogAdapter.Setup(s => s.GetBreadcrumbByProductId("123"))
//                           .Returns(new[]
//                               {
//                                   new ProductCategory {DisplayName = "BC1", CategoryId = "1"},
//                                   new ProductCategory {DisplayName = "BC2", CategoryId = "2"}
//                               });
//            _linkGenerator.Setup(l => l.GenerateCategoryLink("1", false)).Returns("Url1");
//            _linkGenerator.Setup(l => l.GenerateCategoryLink("2", false)).Returns("Url2");
//            IEnumerable<Breadcrumb> cachedCrumbs = null;
//            _breadcrumbCache.Setup(b => b.Add(cacheKey, It.IsAny<IEnumerable<Breadcrumb>>()))
//                            .Callback<CacheKey, IEnumerable<Breadcrumb>>((c, b) =>
//                            { cachedCrumbs = b; });

//            // test
//            var ret = Core.Services.Utils.DependencyResolver.Current.Get<IBreadcrumbViewModelBuilder>().GetBreadcrumbsFromProduct("123");

//            // sense
//            var arr = ret.ToArray();
//            Assert.Equal(2, arr.Length);
//            Assert.Equal("BC1", arr[0].Title);
//            Assert.Equal("Url1", arr[0].Url);
//            Assert.Equal("BC2", arr[1].Title);
//            Assert.Equal("Url2", arr[1].Url);
//            var cache = cachedCrumbs.ToArray();
//            Assert.NotNull(cache);
//            Assert.Equal(2, cache.Length);
//        }

//        // test: GetBreadcrumbsFromProduct caches empty breadcrumbs if no breadcrumbs for category id
//        [Fact]
//        public void GetBreadcrumbsFromProduct_Empty()
//        {
//            // setup
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            IEnumerable<Breadcrumb> res;
//            _breadcrumbCache.Setup(b => b.TryGet(cacheKey, out res)).Returns(false);
//            _catalogAdapter.Setup(s => s.GetBreadcrumbByProductId("123")).Returns((ProductCategory[])null);
//            IEnumerable<Breadcrumb> cachedCrumbs = null;
//            _breadcrumbCache.Setup(b => b.Add(cacheKey, It.IsAny<IEnumerable<Breadcrumb>>()))
//                            .Callback<CacheKey, IEnumerable<Breadcrumb>>((c, b) =>
//                            { cachedCrumbs = b; });

//            // test
//            var ret = Core.Services.Utils.DependencyResolver.Current.Get<IBreadcrumbViewModelBuilder>().GetBreadcrumbsFromProduct("123");

//            // sense
//            var arr = ret.ToArray();
//            Assert.Equal(0, arr.Length);
//            var cache = cachedCrumbs.ToArray();
//            Assert.NotNull(cache);
//            Assert.Equal(0, cache.Length);
//        }
//        #endregion

//        class MyCacheKey : CacheKey
//        {
//            public MyCacheKey(string name)
//            {
//                _parameters.Add(name);
//            }
//        }
//    }
//}
