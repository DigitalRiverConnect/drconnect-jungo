using System.Security.Principal;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Services;
using Moq;
using N2;
using N2.Collections;
using N2.Persistence;
using N2.Security;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers
{
    public class SitemapControllerTests : TestBase
    {
        private readonly Mock<ISecurityManager> _securityManager = new Mock<ISecurityManager>();
        private readonly FakeN2Repository _n2Repository = new FakeN2Repository();
        private readonly Mock<IContentItemRepository> _contentItemRepository = new Mock<IContentItemRepository>();
        private readonly Mock<ICategoryViewModelBuilder> _catViewModelBuilder = new Mock<ICategoryViewModelBuilder>();
        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();

        public SitemapControllerTests()
        {
            DependencyRegistrar.
                StandardDependencies()
                .With(_linkGenerator.Object)
                //.With(_sessionInfoResolver.Object)
                .With(_contentItemRepository.Object)
                .With(_catViewModelBuilder.Object);

            // to break dependency on user security to satisfy ContentItem.GetChildren and AccessFilter:
            _securityManager.Setup(s => s.IsAuthorized(It.IsAny<ContentItem>(), It.IsAny<IPrincipal>())).Returns(true);
            AccessFilter.CurrentSecurityManager = () => _securityManager.Object;

            // to break dependency on N2 database access, be it NHibernate or XML-file-based
            CmsFinder.TheFinder = new ContentFinder(_n2Repository);
        }

        //[Fact]
        //public void GetDependenciesBeatIntoSubmissionForTesting()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel {CategoryId = "c1"});
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.NotNull(res.Entries);
        //    Assert.Equal(0, res.Entries.Count());
        //}

        //// test: doesn't add category if category has no product, including nested category
        //[Fact]
        //public void NoCategoryIfHasNoProducts()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "c1" };
        //    cvm.Items.Add(new CategoryViewModel {CategoryId = "c2"});
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.Equal(0, res.Entries.Count());
        //}

        //// test: adds category if has product and no redirect, including nested category
        //[Fact]
        //public void CategoryIfHasProducts()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel {CategoryId = "c1", DisplayName = "c1 title"};
        //    cvm.Items.Add(new CategoryViewModel {CategoryId = "c2", DisplayName = "c2 title"});
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage, matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c2", null, out matchingCatalogPage)).Returns("/mssg/catalog/c2");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1", out matchingProductPage)).Returns("/mssg/product/p1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p2", out matchingProductPage)).Returns("/mssg/product/p2");
        //    SetupProdSearch("c1", new[] {"p1"});
        //    SetupProdSearch("c2", new[] {"p2"});
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/catalog/c1") == 1);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/catalog/c2") == 1);
        //}

        //// test: doesn't add category if has redirect
        //[Fact]
        //public void NoCategoryIfHasRedirect()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel {CategoryId = "c1", DisplayName = "c1 title"};
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1", out matchingProductPage)).Returns("/mssg/product/p1");
        //    SetupProdSearch("c1", new[] {"p1"});
        //    ctrl.CategoryRedirects.Add("c1", new CategoryRedirect("c1", CategoryRedirectToWhat.CategoryId, "c1red"));
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == "c1 title") == 0);
        //}

        //// test: no endless loop if category exists in tree more than once
        //[Fact]
        //public void NoEndlessLoopIfCatInTreeMultiple()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel {CategoryId = "c1", DisplayName = "c1 title"};
        //    cvm.Items.Add(new CategoryViewModel {CategoryId = "c1", DisplayName = "c1 title"});
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    ctrl.Index();

        //    // nothing to assert -- if we got here, it wasn't an endless loop
        //    // if we didn't get here, fix the dang endless loop
        //}

        //// test: adds products for category including nested category
        //[Fact]
        //public void ProductsForCategory()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "c1", DisplayName = "c1 title" };
        //    cvm.Items.Add(new CategoryViewModel { CategoryId = "c2", DisplayName = "c2 title" });
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage, matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c2", null, out matchingCatalogPage)).Returns("/mssg/catalog/c2");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/product/p1-1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-2", out matchingProductPage)).Returns("/mssg/product/p1-2");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p2-1", out matchingProductPage)).Returns("/mssg/product/p2-1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p2-2", out matchingProductPage)).Returns("/mssg/product/p2-2");
        //    SetupProdSearch("c1", new[] { "p1-1", "p1-2" });
        //    SetupProdSearch("c2", new[] { "p2-1", "p2-2" });
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p1-1") == 1);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p1-2") == 1);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p2-1") == 1);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p2-2") == 1);
        //}

        //// test: excludes product if prod atb "NoIndex" set
        //[Fact]
        //public void ExcludeProductIfNoIndex()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "c1", DisplayName = "c1 title" };
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage, matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/product/p1-1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-2", out matchingProductPage)).Returns("/mssg/product/p1-2");
        //    SetupProdSearch("c1", new[] {"p1-1", "p1-2"}, isNoIndex: new[] {true, false});
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p1-1") == 0);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p1-2") == 1);
        //}

        //// test: excludes product if N2 prop "ExcludeFromSitemap" set
        //[Fact]
        //public void ExcludeProductIfExcludeFromSitemap()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel {CategoryId = "c1", DisplayName = "c1 title"};
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage;
        //    // ReSharper disable once RedundantAssignment
        //    object matchingProductPage1 = new ProductPage {ExcludeFromSitemap = true};
        //    // ReSharper disable once RedundantAssignment
        //    object matchingProductPage2 = new ProductPage {ExcludeFromSitemap = false};
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage1)).Returns("/mssg/product/p1-1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-2", out matchingProductPage2)).Returns("/mssg/product/p1-2");
        //    SetupProdSearch("c1", new[] {"p1-1", "p1-2"});
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p1-1") == 0);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/product/p1-2") == 1);
        //}

        //// test: gets product sitemap props from N2 item
        //[Fact]
        //public void ProductSitemapPropsFromN2Item()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "c1", DisplayName = "c1 title" };
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage;
        //    var aGoodOleDate = new DateTime(1978, 7, 9);
        //    // ReSharper disable once RedundantAssignment
        //    object matchingProductPage = new ProductPage
        //    {
        //        ExcludeFromSitemap = false,
        //        ChangeFrequency = ChangeFrequencyEnum.Hourly,
        //        Priority = 0.2,
        //        Published = aGoodOleDate,
        //        Title = "put something here, but don't care what it is 'cause it's never used"
        //    };
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/product/p1-1");
        //    SetupProdSearch("c1", new[] { "p1-1" });
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    var entry = res.Entries.FirstOrDefault(e => e.Url == "/mssg/product/p1-1");
        //    Assert.NotNull(entry);
        //    Assert.Equal(ChangeFrequencyEnum.Hourly, entry.ChangeFrequency);
        //    Assert.Equal(0.2, entry.Priority);
        //    Assert.Equal(aGoodOleDate, entry.LastModified);
        //    Assert.Equal("put something here, but don't care what it is 'cause it's never used", entry.Title);
        //}

        //// test: excludes category if N2 prop "ExcludeFromSitemap" set
        //[Fact]
        //public void ExcludeCategoryIfExcludeFromSitemap()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "c1", DisplayName = "c1 title" };
        //    cvm.Items.Add(new CategoryViewModel { CategoryId = "c2", DisplayName = "c2 title" });
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    // ReSharper disable once RedundantAssignment
        //    object matchingCatalogPage1 = new CatalogPage { ExcludeFromSitemap = true };
        //    // ReSharper disable once RedundantAssignment
        //    object matchingCatalogPage2 = new CatalogPage { ExcludeFromSitemap = false };
        //    object matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage1)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c2", null, out matchingCatalogPage2)).Returns("/mssg/catalog/c2");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/product/p1-1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p2-1", out matchingProductPage)).Returns("/mssg/product/p2-1");
        //    SetupProdSearch("c1", new[] { "p1-1" });
        //    SetupProdSearch("c2", new[] { "p2-1" });
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/catalog/c1") == 0);
        //    Assert.True(res.Entries.Count(e => e.Url == "/mssg/catalog/c2") == 1);
        //}

        //// test: gets category sitemap props from N2 item
        //[Fact]
        //public void CategorySitemapPropsFromN2Item()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "c1", DisplayName = "c1 title" };
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    var aGoodOleDate = new DateTime(1980, 7, 16);
        //    // ReSharper disable once RedundantAssignment
        //    object matchingCatalogPage = new CatalogPage
        //    {
        //        ExcludeFromSitemap = false,
        //        ChangeFrequency = ChangeFrequencyEnum.Weekly,
        //        Priority = 0.7,
        //        Published = aGoodOleDate,
        //        Title = "any old thing here"
        //    };
        //    object matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("c1", null, out matchingCatalogPage)).Returns("/mssg/catalog/c1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/product/p1-1");
        //    SetupProdSearch("c1", new[] { "p1-1" });
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    var entry = res.Entries.FirstOrDefault(e => e.Url == "/mssg/catalog/c1");
        //    Assert.NotNull(entry);
        //    Assert.Equal(ChangeFrequencyEnum.Weekly, entry.ChangeFrequency);
        //    Assert.Equal(0.7, entry.Priority);
        //    Assert.Equal(aGoodOleDate, entry.LastModified);
        //    Assert.Equal("any old thing here", entry.Title);
        //}

        //// test: excludes content item if not published yet
        //[Fact]
        //public void ExcludeContentItemNotPublished()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.State = ContentState.Published;
        //        c.Published = null;
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem);
        //    var itm2 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci2";
        //        c.State = ContentState.Published;
        //        c.Published = DateTime.Now;
        //        c.Name = "ci2";
        //        c.CategoryID = "cat2";
        //    }, ctrl.RootItem);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 0);
        //    Assert.True(res.Entries.Count(e => e.Title == itm2.Title) == 1);
        //}

        //// test: excludes content item if N2 prop "ExcludeFromSitemap" set
        //[Fact]
        //public void ExcludeContentItemIfExcludeFromSitemap()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //        c.ExcludeFromSitemap = true;
        //    }, ctrl.RootItem);
        //    var itm2 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci2";
        //        c.Name = "ci2";
        //        c.CategoryID = "cat2";
        //        c.ExcludeFromSitemap = false;
        //    }, ctrl.RootItem);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 0);
        //    Assert.True(res.Entries.Count(e => e.Title == itm2.Title) == 1);
        //}

        //// test: excludes generic product / cat pages when traversing content items
        //[Fact]
        //public void ExcludeGenericProdCat()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem);
        //    var itm2 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci2";
        //        c.Name = "ci2";
        //        c.CategoryID = null;
        //    }, ctrl.RootItem);
        //    var itm3 = _n2Repository.Add<ProductPage>(c =>
        //    {
        //        c.Title = "ci3";
        //        c.Name = "ci3";
        //        c.ProductID = "prod1";
        //    }, ctrl.RootItem);
        //    var itm4 = _n2Repository.Add<ProductPage>(c =>
        //    {
        //        c.Title = "ci4";
        //        c.Name = "ci4";
        //        c.ProductID = null;
        //    }, ctrl.RootItem);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 1);
        //    Assert.True(res.Entries.Count(e => e.Title == itm2.Title) == 0);
        //    Assert.True(res.Entries.Count(e => e.Title == itm3.Title) == 1);
        //    Assert.True(res.Entries.Count(e => e.Title == itm4.Title) == 0);
        //}

        //// test: excludes content item product marked as no index
        //[Fact]
        //public void ExcludeNoIndexProdContentItem()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var prod1 = new Product
        //    {
        //        Attributes = new[] {new ExtendedAttribute {Name = "NoIndex", Type = "Boolean", Value = "true"}}
        //    };
        //    _catalogAdapter.Setup(c => c.TryGetProduct("prod1", out prod1))
        //        .Returns(true);
        //    var prodPage1 = _n2Repository.Add<ProductPage>(c =>
        //    {
        //        c.Title = "p1";
        //        c.Name = "p1";
        //        c.ProductID = "prod1";
        //    }, ctrl.RootItem);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == prodPage1.Title) == 0);
        //}

        //// test: ignores N2 parts children
        //[Fact]
        //public void IgnoreNonPages()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem);
        //    itm1.Children.Add(new HtmlPart {ID = 99, Title = "html"});
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 1);
        //    Assert.True(res.Entries.Count(e => e.Title == "html") == 0);
        //}

        //// test: ignores ILanguage pages
        //[Fact]
        //public void IgnoreILanguagePages()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem);
        //    var itm2 = _n2Repository.Add<LanguageRoot>(c =>
        //    {
        //        c.Title = "ci2";
        //        c.Name = "ci2";
        //    }, itm1);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 1);
        //    Assert.True(res.Entries.Count(e => e.Title == itm2.Title) == 0);
        //}

        //// test: adds LanguageRoot's parent's children
        //[Fact]
        //public void AddLanguageRootParentChildren()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem.Parent);
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 1);
        //}

        //// test: handles multi-level tree with cycle in it without going into an endless loop
        ////       this is kind of an aberant situation, but it should handle it nonetheless
        //[Fact]
        //public void ContentItemsWithLoop()
        //{
        //    var ctrl = CreateController();
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>()))
        //        .Returns(new CategoryViewModel { CategoryId = "c1" });
        //    var itm1 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem);
        //    var itm2 = _n2Repository.Add<ProductPage>(c =>
        //    {
        //        c.Title = "ci2";
        //        c.Name = "ci2";
        //        c.ProductID = "prod1";
        //    }, itm1);
        //    var itm3 = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci3";
        //        c.Name = "ci3";
        //        c.CategoryID = "cat2";
        //    }, itm2);
        //    itm3.Children.Add(itm1); // a nasty loop
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.True(res.Entries.Count(e => e.Title == itm1.Title) == 1);
        //    Assert.True(res.Entries.Count(e => e.Title == itm2.Title) == 1);
        //    Assert.True(res.Entries.Count(e => e.Title == itm3.Title) == 1);
        //}

        //// test: don't add url twice, once for cat and once for content item
        //[Fact]
        //public void AddCategoriesOnce()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "cat1", DisplayName = "c1 title" }; // this gets added because of category search
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage = _n2Repository.Add<CatalogPage>(c =>
        //    {
        //        c.Title = "ci1";
        //        c.Name = "ci1";
        //        c.CategoryID = "cat1";
        //    }, ctrl.RootItem); // this gets added because of content item
        //    object matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("cat1", null, out matchingCatalogPage)).Returns(((CatalogPage)matchingCatalogPage).Url);
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/product/p1-1");
        //    SetupProdSearch("cat1", new[] { "p1-1" });
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    var entry = res.Entries.SingleOrDefault(e => e.Title == "ci1");
        //    Assert.NotNull(entry);
        //    Assert.Equal(((CatalogPage)matchingCatalogPage).Url, entry.Url);
        //}

        //// test: don't add url twice, once for prod and once for content item
        //[Fact]
        //public void AddProductsOnce()
        //{
        //    var ctrl = CreateController();
        //    var cvm = new CategoryViewModel { CategoryId = "cat1", DisplayName = "c1 title" };
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingProductPage = _n2Repository.Add<ProductPage>(c =>
        //    {
        //        c.Title = "p1-1 title";
        //        c.Name = "ci1";
        //        c.ProductID = "prod1";
        //    }, ctrl.RootItem); // this gets added because of content item
        //    object matchingCatalogPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("cat1", null, out matchingCatalogPage)).Returns("/mssg/catalog/cat1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns(((ProductPage)matchingProductPage).Url);
        //    SetupProdSearch("cat1", new[] { "p1-1" }); // this gets added because of product search
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    var entry = res.Entries.SingleOrDefault(e => e.Title == "p1-1 title");
        //    Assert.NotNull(entry);
        //    Assert.Equal(((ProductPage)matchingProductPage).Url, entry.Url);
        //}

        //// test: don't add url twice, once for prod and once for content item
        //[Fact]
        //public void DoesNotIncludeOtherLocale()
        //{
        //    var ctrl = CreateController();
        //    _siteResolver.Setup(s => s.ValidateSiteLocale(It.IsAny<string>(), "sg-sg")).Returns(true);
        //    var cvm = new CategoryViewModel { CategoryId = "cat1", DisplayName = "c1 title" }; // this gets added because of category search
        //    _catViewModelBuilder.Setup(c => c.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(cvm);
        //    object matchingCatalogPage, matchingProductPage;
        //    _linkGenerator.Setup(l => l.GenerateCategoryLink("cat1", null, out matchingCatalogPage)).Returns("/mssg/sg-sg/catalog/cat1");
        //    _linkGenerator.Setup(l => l.GenerateProductLink("p1-1", out matchingProductPage)).Returns("/mssg/sg-sg/product/p1-1");
        //    SetupProdSearch("cat1", new[] { "p1-1" });
        //    var res = ctrl.Index() as SiteMapXmlResult;

        //    Assert.NotNull(res);
        //    Assert.Equal(0, res.Entries.Count());
        //}

        //#region internals
        //private MySitemapController CreateController()
        //{
        //    var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MySitemapController>();
        //    SetStandardContext(ctrl);
        //    ctrl.RootItem = GetStandardRootItem();
        //    _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("mssg");
        //    _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("en-sg");
        //    _siteResolver.Setup(s => s.ValidateSiteLocale(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        //    return ctrl;
        //}

        //private void SetupProdSearch(string catid, IList<string> prodids, IList<bool> isDisplables = null, IList<bool> isNoIndex = null)
        //{
        //    var prods = new SearchProduct[prodids.Count];
        //    for (var i = 0; i < prodids.Count; i++)
        //    {
        //        prods[i] = new SearchProduct
        //        {
        //            IsDisplayable = isDisplables == null || isDisplables[i],
        //            Id = prodids[i],
        //            Title = prodids[i] + " title"
        //        };
        //        if (isNoIndex != null)
        //            prods[i].Attributes =
        //                new[] {new ExtendedAttribute {Name = "NoIndex", Type = "Boolean", Value = isNoIndex[i].ToString()}};
        //    }
        //    _catViewModelBuilder.Setup(
        //        c => c.SearchProductByCategoryAsync(catid, It.IsAny<SearchOptions>(), It.IsAny<FacetSearchField[]>()))
        //        .Returns(new CatalogPageViewModel
        //        {
        //            Products = new Products {Products = prods, Count = prods.Length}
        //        });
        //}

        //private void SetStandardContext(ControllerBase ctrl)
        //{
        //    _httpContext.Setup(c => c.Request).Returns(new HttpRequestWrapper(new HttpRequest("test", "http://localhost/mssg/en_sg/sitemap", null)));
        //    ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
        //}

        //private ContentItem GetStandardRootItem()
        //{
        //    return
        //        _n2Repository.Add<LanguageRoot>(c => c.Name = "en_SG",
        //            _n2Repository.Add<StartPage>(c => c.Name = "mssg",
        //                _n2Repository.Add<LanguageIntersection>(c => c.Name = "start",
        //                    _n2Repository.Add<RootBase>(c => c.Name = "root"))));
        //}

        //public class MySitemapController : SitemapController
        //{
        //    public MySitemapController(ICatalogAdapter catalogAdapter, ICategoryViewModelBuilder catViewModelBuilder, ILinkGenerator linkGenerator,
        //        ISessionInfoResolver sessionInfoResolver, ISiteResolver siteResolver)
        //        : base(catalogAdapter, catViewModelBuilder, linkGenerator, sessionInfoResolver, siteResolver)
        //    {
        //        CategoryRedirects = new Dictionary<string, CategoryRedirect>();
        //    }

        //    public ContentItem RootItem { get; set; }
        //    protected override ContentItem GetRootItem(string url)
        //    {
        //        return RootItem;
        //    }

        //    public Dictionary<string,CategoryRedirect> CategoryRedirects { get; private set; }
        //    protected override CategoryRedirect GetCategoryRedirect(long categoryId)
        //    {
        //        return CategoryRedirects.ContainsKey(categoryId) ? CategoryRedirects[categoryId] : null;
        //    }
        //}
        //#endregion
    }
}
