using System;
using System.Security.Principal;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Jungo.Infrastructure;
using Moq;
using N2.Engine;
using N2.Security;
using N2.Web;
using Xunit;
using Xunit.Extensions;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Web.Utils
{
	public class ExtensionsTests : TestBase
	{
        private readonly HttpContextTestData _httpContextTestData = new HttpContextTestData();

		public ExtensionsTests()
		{
			DependencyRegistrar
				.StandardDependencies()
                .With(_httpContextTestData);
		}

		#region IsManaging

		[Theory,
		 InlineData(true, true), // catches built-in N2 test
		 InlineData(false, false)] // anything else is false
		public void IsManaging(bool isSignedIn, bool expected)
		{
			// setup
			var eng = GetEngine(isSignedIn);

			// test
			var isManaging = eng.IsManaging();

			// sense
			Assert.Equal(expected, isManaging);
		}

		[Theory,
		 InlineData("http://my.com/~/N2/stuff", true), // N2 in the path
		 InlineData("http://my.com/abc", false)] // anything else is false
		public void IsManaging_Referrer(string refUrl, bool bExpect)
		{
			// setup
			var eng = GetEngine(false, refUrl);

			// test
			var bIsMg = eng.IsManaging();

			// sense
			Assert.Equal(bExpect, bIsMg);
		}

		private IEngine GetEngine(bool isSignedIn)
		{
			return GetEngine(isSignedIn, string.Empty);
		}

		private IEngine GetEngine(bool isSignedIn, string referrerUrl)
		{
		    var mockIdentity = new Mock<IIdentity>();
		    mockIdentity.SetupGet(mi => mi.IsAuthenticated).Returns(isSignedIn);
			var p = new Mock<IPrincipal>();
			p.Setup(u => u.IsInRole("Administrators")).Returns(true);
		    p.SetupGet(u => u.Identity).Returns(mockIdentity.Object);
			var wc = new Mock<IWebContext>();
			wc.Setup(w => w.Url).Returns(new Url("http://abcdef.com/mypath"));
			wc.Setup(w => w.CurrentPath).Returns(new PathData());
            if(isSignedIn)
			    wc.Setup(w => w.User).Returns(p.Object);
            else
                wc.Setup(w => w.User).Returns((IPrincipal)null);
			var sm = new Mock<ISecurityManager>();
			sm.Setup(s => s.IsEditor(It.IsAny<IPrincipal>())).Returns(true);
			var eng = new Mock<IEngine>();
			eng.Setup(e => e.RequestContext).Returns(wc.Object);
			eng.Setup(e => e.SecurityManager).Returns(sm.Object);
			if (!string.IsNullOrEmpty(referrerUrl))
			{
				var rb = new Mock<HttpRequestBase>();
				rb.Setup(q => q.UrlReferrer).Returns(new Uri(referrerUrl));
				var h = new Mock<HttpContextBase>();
				h.Setup(c => c.Request).Returns(rb.Object);
				wc.Setup(r => r.HttpContext).Returns(h.Object);
			}
			return eng.Object;
		}

		#endregion

        #region AbsoulteUrlWithScheme

        // test: AbsoulteUrlWithScheme force the 'relativeUrl' to an absolute url with the given scheme
	    [Fact]
	    public void AbsoulteUrlWithScheme()
	    {
            // setup
	        _httpContextTestData.Url = "http://MySite.org";
            var request =DependencyResolver.Current.Get<MyHttpContext>().Context.Request;

            // test
            var result = Extensions.AbsoluteUrlWithScheme(request, "http", "/relativeUrl");

            // sense
	        var uri = new Uri(result);
            Assert.Equal("http", uri.Scheme);
            Assert.Equal("mysite.org", uri.Authority);
            Assert.Equal("/relativeUrl", uri.PathAndQuery);
	    }

        // test: AbsoulteUrlWithScheme returns null if 'relativeUrl' is null
        [Fact]
        public void AbsoulteUrlWithScheme_NullRelativeUrl()
        {
            // setup
            _httpContextTestData.Url = "http://MySite.org";
            var request =DependencyResolver.Current.Get<MyHttpContext>().Context.Request;

            // test
            var result = Extensions.AbsoluteUrlWithScheme(request, "http", null);

            // sense
            Assert.Null(result);
        }

        // test: AbsoulteUrlWithScheme will add an absent leading '/' to the relative url
        [Fact]
        public void AbsoulteUrlWithScheme_FixRelativeUrl()
        {
            // setup
            _httpContextTestData.Url = "http://MySite.org";
            var request =DependencyResolver.Current.Get<MyHttpContext>().Context.Request;

            // test
            var result = Extensions.AbsoluteUrlWithScheme(request, "http", "relativeUrl");

            // sense
            var uri = new Uri(result);
            Assert.Equal("/relativeUrl", uri.PathAndQuery);
        }

        // test: AbsoulteUrlWithScheme will force the scheme from https to http
        [Theory,
         InlineData("http", "http"),
         InlineData("https", "http"),
         InlineData("http", "https"),
         InlineData("https", "https")]
        public void AbsoulteUrlWithScheme_SchemeChange(string schemeIn, string schemeOut)
        {
            // setup
            _httpContextTestData.Url = schemeIn + "://MySite.org";
            var request =DependencyResolver.Current.Get<MyHttpContext>().Context.Request;

            // test
            var result = Extensions.AbsoluteUrlWithScheme(request, schemeOut, "relativeUrl");

            // sense
            var uri = new Uri(result);
            Assert.Equal(schemeOut, uri.Scheme);
        }

        // test: AbsoulteUrlWithScheme will force the scheme from https to http
        [Fact]
        public void AbsoulteUrlWithScheme_HttpToHttps()
        {
            // setup
            _httpContextTestData.Url = "http://MySite.org";
            var request =DependencyResolver.Current.Get<MyHttpContext>().Context.Request;

            // test
            var result = Extensions.AbsoluteUrlWithScheme(request, "https", "relativeUrl");

            // sense
            var uri = new Uri(result);
            Assert.Equal("https", uri.Scheme);
        }

	    #endregion

        #region AssureSchemeUrl

        // test: AssureSchemeUrl passed the relative url through unchanged if there is no scheme change
        [Fact]
        public void AssureSchemeUrl_NoSchemeChange()
        {
            // setup
            _httpContextTestData.Url = "http://MySite.org";
            var request =DependencyResolver.Current.Get<MyHttpContext>().Context.Request;

            // test
            var result = Extensions.AssureSchemeUrl(request, "http", "relativeUrl");

            // sense
            Assert.Equal("relativeUrl", result);
        }

        // test: AssureSchemeUrl will change the 'relativeUrl' to an absolute url with the 'targetScheme' if there is a scheme change
        [Theory,
         InlineData("https", "http"),
         InlineData("http", "https")]
        public void AssureSchemeUrl_SchemeChange(string schemeIn, string schemeOut)
        {
            // setup
            _httpContextTestData.Url = schemeIn + "://MySite.org";
            var request = DependencyResolver.Current.Get<MyHttpContext>().Context.Request;
            

            // test
            var result = Extensions.AssureSchemeUrl(request, schemeOut, "relativeUrl");

            // sense
            var uri = new Uri(result);
            Assert.Equal(schemeOut, uri.Scheme);
        }

        #endregion
    }

	public class MyHttpContext
	{
        private readonly Mock<HttpContextBase> _context = new Mock<HttpContextBase>();

        public HttpContextBase Context { get { return _context.Object; } }

        public MyHttpContext(HttpContextTestData myTestData)
        {
            var request = new Mock<HttpRequestBase>();
            if (myTestData != null && !string.IsNullOrEmpty(myTestData.Url))
                request.SetupGet(x => x.Url).Returns(new Uri(myTestData.Url));
            _context.SetupGet(x => x.Request).Returns(request.Object);
        }
	}
}
