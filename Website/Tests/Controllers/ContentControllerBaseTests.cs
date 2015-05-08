using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers;
using Jungo.Infrastructure;
using N2.Web;
using N2.Web.Mvc;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers
{
    public class ContentControllerBaseTests : TestBase
    {
        public ContentControllerBaseTests()
        {
            DependencyRegistrar
                .StandardDependencies();
		}

        [Fact]
        public void GetPageTitleFromCurrentItem_HasTitle()
        {
            var ctrl = DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = "my page title rules the world";

            var res = ctrl.CallGetPageTitleFromCurrentItem();

            Assert.Contains("my page title rules the world", res);
        }

        [Fact]
        public void GetPageTitleFromCurrentItem_NoTitle()
        {
            var ctrl = DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = null;

            var res = ctrl.CallGetPageTitleFromCurrentItem();

            Assert.True(string.IsNullOrEmpty(res));
        }

#if false
        [Fact]
        public void GetPageTitleFromResource_HasResourceForPageType()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            FakeResources.AddString("PageTitle", "PAGETYPE_MyContentItem", "my resource title");

            var res = ctrl.CallGetPageTitleFromResource();

            Assert.Contains("my resource title", res);
        }

        [Fact]
        public void GetPageTitleFromResource_NoResourceForPageType_UsesDefault()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            FakeResources.AddString("PageTitle", "PAGETYPE_MyContentItem", string.Empty);
            FakeResources.AddString("PageTitle", "PAGETYPE_AllOthers", "my all other title");

            var res = ctrl.CallGetPageTitleFromResource();

            Assert.Contains("my all other title", res);
        }

        [Fact]
        public void GetPageTitleFromResource_HasResourceForPageType_RemovesPageSuffixFromTypeName()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBaseWithSuffix>();
            ctrl.MyCurrentPage = new MyContentItemWithSuffixPage();
            FakeResources.AddString("PageTitle", "PAGETYPE_MyContentItemWithSuffix", "my resource title");

            var res = ctrl.CallGetPageTitleFromResource();

            Assert.Contains("my resource title", res);
        }

        [Fact]
        public void GetPageTitleFromResource_HasResourceForOverridePageType()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            FakeResources.AddString("PageTitle", "PAGETYPE_MyOverride", "my override page type title");

            var res = ctrl.CallGetPageTitleFromResource("MyOverride");

            Assert.Contains("my override page type title", res);
        }

        [Fact]
        public void GetPageTitleFromResource_NoResourceForOverridePageType_UsesDefault()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            FakeResources.AddString("PageTitle", "PAGETYPE_MyOverride", string.Empty);
            FakeResources.AddString("PageTitle", "PAGETYPE_AllOthers", "my all other title");

            var res = ctrl.CallGetPageTitleFromResource("MyOverride");

            Assert.Contains("my all other title", res);
        }

        [Fact]
        public void GetPageTitle_FavorsCurrentItem()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = "my content item title";
            FakeResources.AddString("PageTitle", "PAGETYPE_MyContentItem", "my page type title");
            FakeResources.AddString("PageTitle", "PAGETYPE_AllOthers", "my all other title");

            var res = ctrl.CallGetPageTitle();

            Assert.Contains("my content item title", res);
        }

        [Fact]
        public void GetPageTitle_FallsBackToPageTypeResource()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = string.Empty;
            FakeResources.AddString("PageTitle", "PAGETYPE_MyContentItem", "my page type title");
            FakeResources.AddString("PageTitle", "PAGETYPE_AllOthers", "my all other title");

            var res = ctrl.CallGetPageTitle();

            Assert.Contains("my page type title", res);
        }

        [Fact]
        public void GetPageTitle_FallsBackToDefaultResource()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = string.Empty;
            FakeResources.AddString("PageTitle", "PAGETYPE_MyContentItem", string.Empty);
            FakeResources.AddString("PageTitle", "PAGETYPE_AllOthers", "my all other title");

            var res = ctrl.CallGetPageTitle();

            Assert.Contains("my all other title", res);
        }

        [Fact]
        public void SetPageTitleOverrideResKey_CallsGetPageTitleWithOverride()
        {
            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = string.Empty;
            FakeResources.AddString("PageTitle", "PAGETYPE_MyOverride", "my override type title");

            ctrl.CallSetPageTitleOverrideResKey("MyOverride");

            Assert.Contains("my override type title", ctrl.ViewBag.Title);
        }
#endif

        [Fact]
        public void SetPageTitle_SetsViewBag()
        {
            var ctrl = DependencyResolver.Current.Get<MyContentControllerBase>();

            ctrl.CallSetPageTitle("my title");

            Assert.Contains("my title", ctrl.ViewBag.Title);
        }

        [Fact]
        public void SetPageTitleSimple_CallsGetPageTitle()
        {
            var ctrl = DependencyResolver.Current.Get<MyContentControllerBase>();
            ctrl.MyCurrentPage.PageTitle = "my page title";

            ctrl.CallSetPageTitleSimple();

            Assert.Contains("my page title", ctrl.ViewBag.Title);
        }
    }

    class MyContentItem : PageModelBase
    {
    }

    class MyContentControllerBase : ContentControllerBase<MyContentItem>
    {
        public MyContentControllerBase() : base(null)
        {
            Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage));
        }

        public MyContentItem MyCurrentPage = new MyContentItem();

        // promote for calling in tests
        public string CallGetPageTitleFromCurrentItem()
        {
            return GetPageTitleFromCurrentItem();
        }
        public string CallGetPageTitleFromResource(string pageType = null)
        {
            return GetPageTitleFromResource(pageType);
        }
        public string CallGetPageTitle(string pageType = null)
        {
            return GetPageTitle(pageType);
        }
        public void CallSetPageTitle(string title)
        {
            SetPageTitle(title);
        }
        public void CallSetPageTitleSimple()
        {
            SetPageTitleSimple();
        }
        public void CallSetPageTitleOverrideResKey(string pageTypeKey)
        {
            SetPageTitleOverrideResKey(pageTypeKey);
        }
    }

    class MyContentItemWithSuffixPage : MyContentItem
    {
    }

    class MyContentControllerBaseWithSuffix : ContentControllerBase<MyContentItemWithSuffixPage>
    {
        public MyContentControllerBaseWithSuffix() : base(null)
        {
            Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage));
        }

        public MyContentItemWithSuffixPage MyCurrentPage = new MyContentItemWithSuffixPage();

        // promote for calling in tests
        public string CallGetPageTitleFromResource(string pageType = null)
        {
            return GetPageTitleFromResource(pageType);
        }
    }
}
