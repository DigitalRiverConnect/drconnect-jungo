using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Services;
using Moq;
using N2;
using N2.Collections;
using N2.Security;
using N2.Web;
using N2.Web.Mvc;
using Xunit;
using DependencyResolver = Jungo.Infrastructure.DependencyResolver;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Pages
{
    public class CustomRedirectControllerTests : TestBase
    {
        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
        private readonly Mock<ISecurityManager> _securityManager = new Mock<ISecurityManager>();
        private readonly FakeN2Repository _n2Repository = new FakeN2Repository();
        private readonly Mock<HttpContextBase> _httpContext = new Mock<HttpContextBase>();

        public CustomRedirectControllerTests()
        {
            DependencyRegistrar.
                StandardDependencies()
                .With(_linkGenerator.Object);
            _securityManager.Setup(s => s.IsAuthorized(It.IsAny<ContentItem>(), It.IsAny<IPrincipal>())).Returns(true);
            AccessFilter.CurrentSecurityManager = () => _securityManager.Object;

            // to break dependency on N2 database access, be it NHibernate or XML-file-based
            CmsFinder.TheFinder = new ContentFinder(_n2Repository);

            _httpContext.Setup(c => c.Items).Returns(new Dictionary<string, object>());
            _httpContext.Setup(c => c.Request).Returns(new HttpRequestWrapper(new HttpRequest("test", "http://localhost", null)));
        }

        [Fact]
        public void Index_IsManaging_Content()
        {
            var ctrl = DependencyResolver.Current.Get<MyCustomRedirectController>();
            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
            ctrl.IsManagingValue = true;
            ctrl.MyCurrentItem.ExternalUrl = "myExternalUrl";

            var res = ctrl.Index() as ContentResult;

            Assert.NotNull(res);
            Assert.Contains("myExternalUrl", res.Content);
        }

        [Fact]
        public void Index_NoLink_RedirectStore()
        {
            var ctrl = DependencyResolver.Current.Get<MyCustomRedirectController>();
            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
            ctrl.MyCurrentItem.ExternalUrl = string.Empty;
            ctrl.MyCurrentItem.ContentPageName = "mypagename";
            _linkGenerator.Setup(l => l.GenerateLinkForNamedContentItem("mypagename")).Returns(string.Empty);
            _linkGenerator.Setup(l => l.GenerateStoreLink()).Returns("mystoreurl");

            var res = ctrl.Index() as RedirectResult;

            Assert.NotNull(res);
            Assert.Contains("mystoreurl", res.Url);
        }

        [Fact]
        public void Index_Extern_RedirectPermanent()
        {
            var ctrl = DependencyResolver.Current.Get<MyCustomRedirectController>();
            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
            ctrl.MyCurrentItem.ExternalUrl = "myexternalurl";
            ctrl.MyCurrentItem.ContentPageName = string.Empty;

            var res = ctrl.Index() as RedirectResult;

            Assert.NotNull(res);
            Assert.Contains("myexternalurl", res.Url);
            Assert.True(res.Permanent);
        }

        [Fact]
        public void Index_Content_RedirectPermanent()
        {
            var ctrl = DependencyResolver.Current.Get<MyCustomRedirectController>();
            ctrl.ControllerContext = new ControllerContext(_httpContext.Object, new RouteData(), ctrl);
            ctrl.MyCurrentItem.ExternalUrl = string.Empty;
            ctrl.MyCurrentItem.ContentPageName = "mypagename";
            _linkGenerator.Setup(l => l.GenerateLinkForNamedContentItem("mypagename")).Returns("mycontenturl");

            var res = ctrl.Index() as RedirectResult;

            Assert.NotNull(res);
            Assert.Contains("mycontenturl", res.Url);
            Assert.True(res.Permanent);
        }

        [Fact]
        public void CustomRedirectPage_GetContentPageTitle()
        {
            var item = _n2Repository.Add<CustomRedirectPage>(c => c.ContentPageName = "mycontentpagename");
            _n2Repository.Add<CatalogPage>(c =>
            {
                c.Name = "mycontentpagename";
                c.Title = "My Content Page Title";
            });

            // can retrieve referred-to content page title if not known in the referring object
            Assert.Equal("My Content Page Title", item.ContentPageTitle);
        }
    }

    public class MyCustomRedirectController : CustomRedirectController
    {
        public MyCustomRedirectController(ILinkGenerator linkGenerator)
            : base(linkGenerator, null)
        {
            Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage, MyCurrentItem));
        }

        public CustomRedirectPage MyCurrentItem = new CustomRedirectPage();
        public CustomRedirectPage MyCurrentPage = new CustomRedirectPage();

        protected override bool IsManaging
        {
            get { return IsManagingValue; }
        }

        public bool IsManagingValue { set; private get; }
    }
}
