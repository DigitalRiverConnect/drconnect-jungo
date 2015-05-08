namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers
{
#if false
    public class AdminControllerTests : TestBase
    {
        private readonly Mock<ISecurityManager> _securityManager = new Mock<ISecurityManager>();
        private readonly FakeN2Repository _n2Repository = new FakeN2Repository();
        private readonly Mock<IAdministrationService> _administrationService = new Mock<IAdministrationService>();
        private readonly Mock<IContentItemRepository> _contentItemRepository = new Mock<IContentItemRepository>();
        private readonly Mock<ICacheService> _cacheService = new Mock<ICacheService>();

        public AdminControllerTests()
        {
            DependencyRegistrar.
                StandardDependencies()
                .With(_cacheService.Object)
                .With(_administrationService.Object)
                .With(_contentItemRepository.Object);

            // to break dependency on user security to satisfy ContentItem.GetChildren and AccessFilter:
            _securityManager.Setup(s => s.IsAuthorized(It.IsAny<ContentItem>(), It.IsAny<IPrincipal>())).Returns(true);
            AccessFilter.CurrentSecurityManager = () => _securityManager.Object;

            // to break dependency on N2 database access, be it NHibernate or XML-file-based
            CmsFinder.TheFinder = new ContentFinder(_n2Repository);
		}

        [Fact]
        public void ForceReplication_NoSite()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyAdminController>();

            var res = ctrl.ForceReplication(null) as ContentResult;

            Assert.NotNull(res);
            Assert.Contains("supply the site ID", res.Content);
        }

        [Fact]
        public void ForceReplication_BadSite()
        {
            var langInt = _n2Repository.Add<LanguageIntersection>();
            _n2Repository.Add<StartPage>(s => s.SiteID = "sp1", langInt);
            _n2Repository.Add<StartPage>(s => s.SiteID = "sp2", langInt);
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyAdminController>();

            var res = ctrl.ForceReplication("sp3") as ContentResult;

            Assert.NotNull(res);
            Assert.Contains("not a valid site", res.Content);
        }

        [Fact]
        public void ForceReplication_OneSite()
        {
            var langInt = _n2Repository.Add<LanguageIntersection>();
            SomeContentStartCatProdProd(langInt, "mssg");
            SomeContentStartCatProdProd(langInt, "not-mssg");
            var bFlushCalled = false;
            _contentItemRepository.Setup(c => c.Flush()).Callback(() => bFlushCalled = true);
            var changeList = new List<ContentItem>();
            _contentItemRepository.Setup(c => c.SaveOrUpdate(It.IsAny<ContentItem>()))
                .Callback<ContentItem>(changeList.Add);
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyAdminController>();

            var res = ctrl.ForceReplication("mssg") as ContentResult;

            Assert.NotNull(res);
            Assert.True(bFlushCalled);
            Assert.Equal(4, changeList.Count);
            foreach (var page in changeList)
            {
                Assert.True(page.Name.StartsWith("mssg_")); // no other site updated
                Assert.True(page.Published.HasValue);
                Assert.Equal(_initialPubDate.AddSeconds(1.0), page.Published.Value); // time advanced one second
            }
            Assert.Contains(">mssg<", res.Content);
            Assert.Contains(">4<", res.Content);
        }

        [Fact]
        public void ForceReplication_AllSites()
        {
            var langInt = _n2Repository.Add<LanguageIntersection>();
            SomeContentStartCatProdProd(langInt, "mssg");
            SomeContentStartCatProdProd(langInt, "not-mssg");
            var bFlushCalled = false;
            _contentItemRepository.Setup(c => c.Flush()).Callback(() => bFlushCalled = true);
            var changeList = new List<ContentItem>();
            _contentItemRepository.Setup(c => c.SaveOrUpdate(It.IsAny<ContentItem>()))
                .Callback<ContentItem>(changeList.Add);
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyAdminController>();

            var res = ctrl.ForceReplication("all") as ContentResult;

            Assert.NotNull(res);
            Assert.True(bFlushCalled);
            Assert.Equal(8, changeList.Count);
            foreach (var page in changeList)
            {
                Assert.True(page.Published.HasValue);
                Assert.Equal(_initialPubDate.AddSeconds(1.0), page.Published.Value); // time advanced one second
            }
            Assert.Contains(">mssg</td><td class='numcol'>4<", res.Content);
            Assert.Contains(">not-mssg</td><td class='numcol'>4<", res.Content);
        }

        private readonly DateTime _initialPubDate = new DateTime(2014, 12, 25, 15, 16, 17);

        private void SomeContentStartCatProdProd(ContentItem parent, string site)
        {
            var startPage =
                _n2Repository.Add<StartPage>(
                    s =>
                    {
                        s.Name = site + "_" + s.ID;
                        s.SiteID = site;
                        s.Published = _initialPubDate;
                    }, parent);
            var catPage1 = _n2Repository.Add<CatalogPage>(
                c =>
                {
                    c.Published = _initialPubDate;
                    c.Name = site + "_" + c.ID;
                }, startPage);
            _n2Repository.Add<ProductPage>(
                p =>
                {
                    p.Published = _initialPubDate;
                    p.Name = site + "_" + p.ID;
                }, catPage1);
            _n2Repository.Add<ProductPage>(
                p =>
                {
                    p.Published = _initialPubDate;
                    p.Name = site + "_" + p.ID;
                }, catPage1);
        }
    }

    // to break dependency on config-file-based injection of ICacheService
    public class MyAdminController : AdminController
    {
        public MyAdminController(ICacheService cacheService, IAdministrationService administrationService, IContentItemRepository contentItemRepository)
            : base(cacheService, administrationService, contentItemRepository)
        {
        }
    }
#endif
}
