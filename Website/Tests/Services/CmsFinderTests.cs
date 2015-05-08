using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Moq;
using N2;
using N2.Collections;
using N2.Management.Myself;
using N2.Persistence.Finder;
using N2.Security;
using Shouldly;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Services
{
    public class CmsFinderTests : TestBase
    {
	    //~~ private static N2.Engine.IEngine _engine;
        private readonly Mock<ISessionInfoResolver> _sessionInfoResolver = new Mock<ISessionInfoResolver>();
        private readonly Mock<ISecurityManager> _securityManager = new Mock<ISecurityManager>();
        private readonly FakeN2Repository _n2Repository = new FakeN2Repository();

        public CmsFinderTests()
		{
            // to break dependency on user security to satisfy ContentItem.GetChildren and AccessFilter:
            _securityManager.Setup(s => s.IsAuthorized(It.IsAny<ContentItem>(), It.IsAny<IPrincipal>())).Returns(true);
            AccessFilter.CurrentSecurityManager = () => _securityManager.Object;

            // to break dependency on N2 database access, be it NHibernate or XML-file-based
            CmsFinder.TheFinder = new ContentFinder(_n2Repository);

            // to break dependency on N2 initializing an engine
            CmsFinder.SetSessionInfoResolver(_sessionInfoResolver.Object);
		}

        // FIRST, we test the implementation of IContentFinder used by CmsFinder.
        //        This ContentFinder depends on IItemFinder for culling through the N2 database.
        //        Since we're not testing here whether N2's IItemFinder implementations work, we fake the IItemFinder
        //          to break the dependency on an N2 database being loaded.
        //   NOTE: Because the IItemFinder is injected at runtime by N2 configuration rather than our Ninject test configuration, we
        //         "new up" our own IItemFinder and pass it directly to the ContentFinder's c-tor.
        // THEN, once we're sure ContentFinder works properly, all subsequent tests of CmsFinder use the faked IItemFinder.

        #region ContentFinder
        // all current uses of IContentFinder.FindAll pass null for the filter parameters,
        // so until we have uses of non-null filters, we only need to test the null case.
        [Fact]
        public void ContentFinder_FindAll_WithDefaultFilters()
        {
            _n2Repository.Add<LanguageIntersection>( // published yesterday
                l =>
                {
                    l.Name = "1";
                    l.State = ContentState.Published;
                    l.Expires = null;
                    l.Published = Utility.CurrentTime().AddDays(-1);
                });
            _n2Repository.Add<LanguageIntersection>( // draft
                l =>
                {
                    l.Name = "2";
                    l.State = ContentState.Draft;
                });
            _n2Repository.Add<LanguageIntersection>( // published just now
                l =>
                {
                    l.Name = "3";
                    l.State = ContentState.Published;
                    l.Expires = null;
                    l.Published = Utility.CurrentTime();
                });
            _n2Repository.Add<LanguageIntersection>( // published but expired
                l =>
                {
                    l.Name = "4";
                    l.State = ContentState.Published;
                    l.Expires = Utility.CurrentTime().AddDays(-1);
                    l.Published = Utility.CurrentTime();
                });

            var res = CmsFinder.TheFinder.FindAll<LanguageIntersection>().ToList();
                // have to force enumeration to get results of find

            Assert.Equal(2, res.Count);
            Assert.Equal("1", res[0].Name);
            Assert.Equal("3", res[1].Name);
        }

        // all current uses of IContentFinder.FindAllDescendentsOf pass null for the filter parameters,
        // so until we have uses of non-null filters, we only need to test the null case.
        [Fact]
        public void ContentFinder_FindAllDescendentsOf_WithNoFilters()
        {
            var root = _n2Repository.Add<RootBase>();
            var langInt1 = _n2Repository.Add<LanguageIntersection>(root);
            _n2Repository.Add<StartPage>(s => s.Name = "sp1-under-li1", langInt1);
            _n2Repository.Add<StartPage>(s => s.Name = "sp2-under-li1", langInt1);
            var langInt2 = _n2Repository.Add<LanguageIntersection>(root);
            _n2Repository.Add<StartPage>(s => s.Name = "sp3-under-li2", langInt2);

            var res1 = CmsFinder.TheFinder.FindAllDescendentsOf<StartPage>(langInt1).ToList(); // have to force enumeration to get results of find
            var res2 = CmsFinder.TheFinder.FindAllDescendentsOf<StartPage>(langInt2).ToList(); // have to force enumeration to get results of find

            Assert.Equal(2, res1.Count);
            Assert.Equal(1, res2.Count);
            Assert.Equal("sp1-under-li1", res1[0].Name);
            Assert.Equal("sp2-under-li1", res1[1].Name);
            Assert.Equal("sp3-under-li2", res2[0].Name);
        }
        #endregion

        #region usages of N2's GenericFind, not dependent on N2's database, but only needs an existing ContentItem and parentage
        [Fact]
        public void FindLanguageRootOf()
        {
            var productPage =
                _n2Repository.Add<ProductPage>(
                    _n2Repository.Add<CatalogPage>(
                        _n2Repository.Add<LanguageRoot>(l => l.Name = "12345",
                            _n2Repository.Add<StartPage>(
                                _n2Repository.Add<LanguageIntersection>(
                                    _n2Repository.Add<RootBase>())))));

            var result = CmsFinder.FindLanguageRootOf(productPage);

            Assert.NotNull(result);
            result.ShouldBeTypeOf<LanguageRoot>();
            Assert.Equal("12345", result.Name);
        }

        [Fact]
        public void FindStartPageOf()
        {
            var productPage =
                _n2Repository.Add<ProductPage>(
                    _n2Repository.Add<CatalogPage>(
                        _n2Repository.Add<LanguageRoot>(
                            _n2Repository.Add<StartPage>(s => s.Name = "12345",
                                _n2Repository.Add<LanguageIntersection>(
                                    _n2Repository.Add<RootBase>())))));

            var result = CmsFinder.FindStartPageOf(productPage);

            Assert.NotNull(result);
            result.ShouldBeTypeOf<StartPage>();
            Assert.Equal("12345", result.Name);
        }
        #endregion

        #region trivial wrappings of ContentFinder
        [Fact]
        public void WrapCF_FindAllDescendentsOf()
        {
            // we don't really need to test CmsFinder.FindAllDescendentsOf very thoroughly,
            // because it's merely a forward to IContentFinder.FindAllDescendentsOf
            var somePage = _n2Repository.Add<LanguageIntersection>();
            _n2Repository.Add<StartPage>(s => s.Name = "1", somePage);
            _n2Repository.Add<StartPage>(s => s.Name = "2", somePage);

            var res = CmsFinder.FindAllDescendentsOf<StartPage>(somePage);

            // just make sure it was a straight pass-thru to IContentFinder
            Assert.Equal(2, res.Count);
            Assert.Equal("1", res[0].Name);
            Assert.Equal("2", res[1].Name);
        }

        [Fact]
        public void WrapCF_FindTranslationsOf()
        {
            // we don't really need to test CmsFinder.FindTranslationsOf very thoroughly,
            // because it's merely a forward to IContentFinder.FindAllDescendentsOf
            var somePage = _n2Repository.Add<LanguageIntersection>();
            _n2Repository.Add<LanguageRoot>(s => s.Name = "1", somePage);
            _n2Repository.Add<LanguageRoot>(s => s.Name = "2", somePage);

            var res = CmsFinder.FindTranslationsOf(somePage).ToList();

            // just make sure it was a straight pass-thru to IContentFinder
            Assert.Equal(2, res.Count);
            Assert.Equal("1", res[0].Name);
            Assert.Equal("2", res[1].Name);
        }

        [Fact]
        public void WrapCF_FindLanguageIntersection_None()
        {
            var res = CmsFinder.FindLanguageIntersection();

            Assert.Null(res);
        }

        [Fact]
        public void WrapCF_FindLanguageIntersection_One()
        {
            _n2Repository.Add<LanguageIntersection>(l => l.Name = "1");

            var res = CmsFinder.FindLanguageIntersection();

            Assert.NotNull(res);
            Assert.Equal("1", res.Name);
        }

        [Fact]
        public void WrapCF_FindLanguageIntersection_FirstOfMany()
        {
            _n2Repository.Add<LanguageIntersection>(l => l.Name = "1");
            _n2Repository.Add<LanguageIntersection>(l => l.Name = "2");

            var res = CmsFinder.FindLanguageIntersection();

            Assert.NotNull(res);
            Assert.Equal("1", res.Name);
        }
        #endregion

        #region FindAllNonSiteDescendentsOfRoot [abbrev. FindNonSiteDesc]

        private void SetupFindNonSiteDesc()
        {
            var langInt =
                _n2Repository.Add<LanguageIntersection>(
                    _n2Repository.Add<RootBase>());

            // NOT descended from a start page, so are candidate choices
            _n2Repository.Add<CatalogPage>(c => c.CategoryID = "3x", langInt);
            _n2Repository.Add<CatalogPage>(c => c.CategoryID = "4", langInt);

            // ARE descended from a start page, so should not be candidate choices
            _n2Repository.Add<CatalogPage>(c => c.CategoryID = "6x",
                _n2Repository.Add<StartPage>(langInt));
            _n2Repository.Add<CatalogPage>(c => c.CategoryID = "8",
                _n2Repository.Add<StartPage>(langInt));
        }

        [Fact]
        public void FindNonSiteDesc()
        {
            SetupFindNonSiteDesc();

            var res = CmsFinder.FindAllNonSiteDescendentsOfRoot<CatalogPage>();

            Assert.Equal(2, res.Count);
            Assert.Equal("3x", res[0].CategoryID);
            Assert.Equal("4", res[1].CategoryID);
        }

        [Fact]
        public void FindNonSiteDesc_Pred()
        {
            SetupFindNonSiteDesc();

            var res = CmsFinder.FindAllNonSiteDescendentsOfRoot<CatalogPage>(c => c.CategoryID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("3x", res[0].CategoryID);
        }

        #endregion

        #region FindDescendentsOrFallbackOfCurrentPageLanguageRoot [abbrev. FindDescOrFallback]

        [Fact]
        public void FindDescOrFallback_HaveCrnt_DescOfLangRoot()
        {
            // current page has ancestor language root
            // said language root has descendent pages of requested type
            var langRoot = _n2Repository.Add<LanguageRoot>();
            _n2Repository.Add<ProductPage>(p => p.Name = "2", langRoot);
            var catPage = _n2Repository.Add<CatalogPage>(langRoot);
            _n2Repository.Add<ProductPage>(p => p.Name = "4", catPage);

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage);

            Assert.Equal(2, res.Count);
            Assert.Equal("2", res[0].Name);
            Assert.Equal("4", res[1].Name);
        }

        [Fact]
        public void FindDescOrFallback_HaveCrnt_DescOfLangRoot_Pred()
        {
            var langRoot = _n2Repository.Add<LanguageRoot>();
            _n2Repository.Add<ProductPage>(p => p.ProductID = "2", langRoot);
            var catPage = _n2Repository.Add<CatalogPage>(langRoot);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4x", catPage);

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage,
                p => p.ProductID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("4x", res[0].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_HaveCrnt_DescOfNonLang()
        {
            // current page has no ancestor language root (condition not tested: current page has language root, but language root has no pages of requested type)
            // current page has ancestor start page
            // said start page has descendent pages of requested type
            //  and some of those descendent pages are excluded from the result because they descend from a language root
            var startPage = _n2Repository.Add<StartPage>();
            var catPage = _n2Repository.Add<CatalogPage>(startPage);
            _n2Repository.Add<ProductPage>(p => p.Name = "3", catPage);
            _n2Repository.Add<ProductPage>(p => p.Name = "4", catPage);
            _n2Repository.Add<ProductPage>(
                _n2Repository.Add<LanguageRoot>(catPage));

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage);

            Assert.Equal(2, res.Count);
            Assert.Equal("3", res[0].Name);
            Assert.Equal("4", res[1].Name);
        }

        [Fact]
        public void FindDescOrFallback_HaveCrnt_DescOfNonLang_Pred()
        {
            var startPage = _n2Repository.Add<StartPage>();
            var catPage = _n2Repository.Add<CatalogPage>(startPage);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "3", catPage);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4x", catPage);
            _n2Repository.Add<ProductPage>(
                _n2Repository.Add<LanguageRoot>(catPage));

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage,
                p => p.ProductID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("4x", res[0].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_HaveCrnt_DescOfNonSite()
        {
            // current page has no ancestor language root
            // current page has start page, but start page has no descendent pages of requested type
            // tree has at least one language intersection
            // first of said language intersections has descendents of requested type not descended from a start page
            var langInt = _n2Repository.Add<LanguageIntersection>();
            var catPage1 = _n2Repository.Add<CatalogPage>(langInt);
            _n2Repository.Add<ProductPage>(p => p.Name = "3", catPage1);
            _n2Repository.Add<ProductPage>(p => p.Name = "4", catPage1);
            _n2Repository.Add<ProductPage>(
                _n2Repository.Add<StartPage>(langInt));
            var catPage2 = _n2Repository.Add<CatalogPage>(
                _n2Repository.Add<StartPage>(langInt));

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage2);

            Assert.Equal(2, res.Count);
            Assert.Equal("3", res[0].Name);
            Assert.Equal("4", res[1].Name);
        }

        [Fact]
        public void FindDescOrFallback_HaveCrnt_DescOfNonSite_Pred()
        {
            var langInt = _n2Repository.Add<LanguageIntersection>();
            var catPage1 = _n2Repository.Add<CatalogPage>(langInt);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "3", catPage1);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4x", catPage1);
            _n2Repository.Add<ProductPage>(
                _n2Repository.Add<StartPage>(langInt));
            var catPage2 = _n2Repository.Add<CatalogPage>(
                _n2Repository.Add<StartPage>(langInt));

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage2,
                p => p.ProductID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("4x", res[0].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_HaveCrnt_NoOtherQualDesc()
        {
            // current page has no ancestor language root
            // current page has start page, but start page has no descendent pages of requested type
            // tree has at least one language intersection, but it has no descendents of requested type not descended from a start page
            var langInt = _n2Repository.Add<LanguageIntersection>();
            var startPage1 = _n2Repository.Add<StartPage>(langInt);
            _n2Repository.Add<ProductPage>(startPage1);
            _n2Repository.Add<ProductPage>(startPage1);
            var catPage2 = _n2Repository.Add<CatalogPage>(
                _n2Repository.Add<StartPage>(langInt));

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(catPage2);

            Assert.Equal(0, res.Count);
        }

        // TODO: all these FindDescOrFallback_NoCrnt_... tests are for current behavior, but the current behavior is probably WRONG!!!
        //       see TODO comment in CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot

        [Fact]
        public void FindDescOrFallback_NoCrnt_NoStart()
        {
            // no current page
            // no start page for current site id
            _n2Repository.Add<ProductPage>(
                _n2Repository.Add<StartPage>(s => s.SiteID = "s2",
                    _n2Repository.Add<LanguageIntersection>()));
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>((ContentItem) null);

            Assert.Equal(0, res.Count);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_NoLangRoot()
        {
            // no current page
            // has start page for current site id
            // said start page has no language root for current language code
            _n2Repository.Add<ProductPage>(
                _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1",
                    _n2Repository.Add<StartPage>(s => s.SiteID = "s1",
                        _n2Repository.Add<LanguageIntersection>())));
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc2");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>((ContentItem) null);

            Assert.Equal(0, res.Count);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_DescOfLangRoot()
        {
            // no current page
            // has start page for current site id and language root for current language code
            // said language root has descendents of requested type
            var langRoot =
                _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1",
                    _n2Repository.Add<StartPage>(s => s.SiteID = "s1",
                        _n2Repository.Add<LanguageIntersection>()));
            _n2Repository.Add<ProductPage>(p => p.Name = "4", langRoot);
            _n2Repository.Add<ProductPage>(p => p.Name = "5", langRoot);
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>((ContentItem) null);

            Assert.Equal(2, res.Count);
            Assert.Equal("4", res[0].Name);
            Assert.Equal("5", res[1].Name);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_DescOfLangRoot_Pred()
        {
            var langRoot =
                _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1",
                    _n2Repository.Add<StartPage>(s => s.SiteID = "s1",
                        _n2Repository.Add<LanguageIntersection>()));
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4", langRoot);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "5x", langRoot);
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns(langRoot.LanguageCode);

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(null,
                p => p.ProductID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("5x", res[0].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_DescOfStart()
        {
            // no current page
            // has start page for current site id and language root for current language code
            // said language root has no descendents of requested type, but start page does
            var startPage =
                _n2Repository.Add<StartPage>(s => s.SiteID = "s1",
                    _n2Repository.Add<LanguageIntersection>());
            _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1", startPage);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4", startPage);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "5", startPage);
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>((ContentItem)null);

            Assert.Equal(2, res.Count);
            Assert.Equal("4", res[0].ProductID);
            Assert.Equal("5", res[1].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_DescOfStart_Pred()
        {
            var startPage =
                _n2Repository.Add<StartPage>(s => s.SiteID = "s1",
                    _n2Repository.Add<LanguageIntersection>());
            _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1", startPage);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4", startPage);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "5x", startPage);
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(null,
                p => p.ProductID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("5x", res[0].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_DescOfNonSite()
        {
            // no current page
            // has start page for current site id and language root for current language code
            // neither said language root nor start page have descendents of requested type
            // but root does
            var langInt = _n2Repository.Add<LanguageIntersection>();
            _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1",
                _n2Repository.Add<StartPage>(s => s.SiteID = "s1", langInt));
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4", langInt);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "5", langInt);
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>((ContentItem)null);

            Assert.Equal(2, res.Count);
            Assert.Equal("4", res[0].ProductID);
            Assert.Equal("5", res[1].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_DescOfNonSite_Pred()
        {
            var langInt = _n2Repository.Add<LanguageIntersection>();
            _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1",
                _n2Repository.Add<StartPage>(s => s.SiteID = "s1", langInt));
            _n2Repository.Add<ProductPage>(p => p.ProductID = "4", langInt);
            _n2Repository.Add<ProductPage>(p => p.ProductID = "5x", langInt);
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>(null,
                p => p.ProductID.EndsWith("x"));

            Assert.Equal(1, res.Count);
            Assert.Equal("5x", res[0].ProductID);
        }

        [Fact]
        public void FindDescOrFallback_NoCrnt_NoOtherQualDesc()
        {
            // no current page
            // has start page for current site id and language root for current language code
            // neither said language root nor start page have descendents of requested type
            // nothing else has any either
            var langInt = _n2Repository.Add<LanguageIntersection>();
            _n2Repository.Add<LanguageRoot>(l => l.LanguageCode = "lc1",
                _n2Repository.Add<StartPage>(s => s.SiteID = "s1", langInt));
            _sessionInfoResolver.SetupGet(s => s.SiteId).Returns("s1");
            _sessionInfoResolver.SetupGet(s => s.CultureCode).Returns("lc1");

            var res = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>((ContentItem)null);

            Assert.Equal(0, res.Count);
        }
        #endregion

#if false
        // these are tests of CultureHelper, not of CmsFinder;
        //  move out of here,
        //  then break dependencies on N2 database usage due to these tests calling Find.StartPage
        //    and due to SelectLanguage calling Find.ClosestOf

        private static string[] userlangs = { "en-us; q=0.8", "de-de" };
        private static string[] userlangs2 = { "de-at; q=0.8", "fr-be" };

        [Fact(Skip = "see comment above")]
        public void Test_SelectLanguage()
        {
            var page = CultureHelper.SelectLanguage(Find.StartPage, userlangs);
            page.ShouldNotBe(null);
            var lroot = page as LanguageRoot;
            lroot.ShouldNotBe(null);
            lroot.LanguageCode.ShouldBe("en-US");
        }

        [Fact(Skip = "see skip reason for Test_SelectLanguage")]
        public void Test_SelectLanguage_Null()
        {
            var page = CultureHelper.SelectLanguage(Find.StartPage, null);
            page.ShouldNotBe(null);
            var lroot = page as LanguageRoot;
            lroot.ShouldNotBe(null);
            lroot.LanguageCode.ShouldBe("en-US");
        }

        [Fact(Skip = "see skip reason for Test_SelectLanguage")]
        public void Test_SelectLanguage_PartialMatch()
        {
            var page = CultureHelper.SelectLanguage(Find.StartPage, userlangs2);
            page.ShouldNotBe(null);
            var lroot = page as LanguageRoot;
            lroot.ShouldNotBe(null);
            lroot.LanguageCode.ShouldBe("fr-CA");
        }
#endif
    }

    public class FakeN2Repository : IItemFinder
    {
        private readonly List<ContentItem> _repo = new List<ContentItem>();

        public T Add<T>(Action<T> initializer, ContentItem parent) where T : ContentItem, new()
        {
            var c = new T {Parent = parent, AncestralTrail = parent.GetTrail(), ID = _repo.Count + 1};
            if (parent != null)
                parent.Children.Add(c);
            if (initializer != null)
                initializer(c);
            _repo.Add(c);
            return c;
        }

        public T Add<T>(Action<T> initializer) where T : ContentItem, new()
        {
            return Add(initializer, null);
        }

        public T Add<T>(ContentItem parent) where T : ContentItem, new()
        {
            return Add((Action<T>)null, parent);
        }

        public T Add<T>() where T : ContentItem, new()
        {
            return Add((Action<T>)null, null);
        }

        #region IItemFinder Members

        public IQueryBuilder Where
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryEnding All
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<T> AllOfType<T>() where T : ContentItem
        {
            return _repo.OfType<T>();
        }

        #endregion
    }
}
