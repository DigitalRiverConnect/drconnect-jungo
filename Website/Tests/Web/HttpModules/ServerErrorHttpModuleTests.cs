using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure;
using Jungo.Infrastructure;
using Moq;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Web.HttpModules
{
    public class ServerErrorHttpModuleTests : TestBase
    {
        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();

        public ServerErrorHttpModuleTests()
        {
            DependencyRegistrar
                .StandardDependencies()
                .WithFakeLogger()
                .With(_linkGenerator.Object);
        }

        #region EncodeError(HttpContext)
        [Fact]
        public void EncodeError_FromHttpContext()
        {
            // setup
            const string url = "http://myserver.com/site/page?param=value";
            var x = new HttpContext(new HttpRequest("file", url, "param=value"), new HttpResponse(TextWriter.Null));
            var guid = Guid.NewGuid();
            x.Items.Add("loggingId", guid);
            const string msg = "booboo";
            Exception exc;
            try
            {
                throw new Exception(msg); // to get a stack trace
            }
            catch (Exception ex)
            {
                exc = ex;
            }
            x.AddError(exc);

            // test
            var res = ServerErrorHttpModule.EncodeError(x);

            // sense
            var dec = Encoding.UTF8.GetString(Convert.FromBase64String(res));
            Assert.False(string.IsNullOrEmpty(dec));
            var parts = dec.Split(ServerErrorHttpModule.ErrorSepChar);
            Assert.Equal(4, parts.Length);
            Assert.True(parts.Contains(guid.ToString()));
            Assert.True(parts.Contains(url));
            Assert.True(parts.Contains(msg));
            Assert.Equal(1, parts.Count(p => p.Contains("EncodeError_FromHttpContext")));
        }
        #endregion

        #region EncodeError(string,string,string,string)
        [Fact]
        public void EncodeError_Parts()
        {
            // test
            var res = ServerErrorHttpModule.EncodeError("1", "2", "3", "4");

            // sense
            var dec = Encoding.UTF8.GetString(Convert.FromBase64String(res));
            Assert.False(string.IsNullOrEmpty(dec));
            var parts = dec.Split(ServerErrorHttpModule.ErrorSepChar);
            Assert.Equal(4, parts.Length);
            Assert.True(parts.Contains("1"));
            Assert.True(parts.Contains("2"));
            Assert.True(parts.Contains("3"));
            Assert.True(parts.Contains("4"));
        }
        #endregion

        #region DecodeErrror
        [Fact]
        public void DecodeError_RoundTrip()
        {
            // test
            string p1, p2, p3, p4;
            ServerErrorHttpModule.DecodeError(ServerErrorHttpModule.EncodeError("1", "2", "3", "4"), out p1, out p2, out p3, out p4);

            // sense
            Assert.Equal("1", p1);
            Assert.Equal("2", p2);
            Assert.Equal("3", p3);
            Assert.Equal("4", p4);
        }
        #endregion

        #region OnError
        [Fact(Skip = "Not found using pure MVC method for now.")]
        public void OnError_NotFound()
        {
            // setup
            var x = NewHttpContext("http://myserver.com/site/page?param=value");
            x.AddError(new HttpException("blah blah blah was not found or does not implement IController yada yada"));
            //_linkGenerator.Setup(l => l.GenerateNotFoundLink()).Returns("/where/oh/where/has/my/little/dog/gone");
            var module =DependencyResolver.Current.Get<MyServerErrorHttpModule>();

            // test
            module.CallOnError(x);

            // sense
            Assert.True(x.Response.IsRequestBeingRedirected);
            Assert.Equal("/where/oh/where/has/my/little/dog/gone", x.Response.RedirectLocation);
        }

        [Fact(Skip = "Using pure mvc error for now.")]
        public void OnError_IgnoresOn2Errors()
        {
            // setup
            var x = NewHttpContext("http://myserver.com/site/page?param=value");
            x.AddError(new Exception("error 1"));
            x.AddError(new Exception("error 2"));
            //_linkGenerator.Setup(l => l.GenerateServerErrorLink()).Throws(new Exception("oops"));
            var module =DependencyResolver.Current.Get<MyServerErrorHttpModule>();

            // test
            module.CallOnError(x);

            // sense
            Assert.False(x.Response.IsRequestBeingRedirected);
        }

        [Fact(Skip = "Using MVC error handling for now.")]
        public void OnError_IgnoresOnNoCustomErrorPage()
        {
            // setup
            const string url = "http://myserver.com/site/errorpage";
            var x = NewHttpContext(url);
            x.AddError(new Exception("error 1"));
            //_linkGenerator.Setup(l => l.GenerateServerErrorLink()).Returns(string.Empty);
            var module =DependencyResolver.Current.Get<MyServerErrorHttpModule>();

            // test
            module.CallOnError(x);

            // sense
            Assert.False(x.Response.IsRequestBeingRedirected);
        }

        [Fact(Skip = "Using MVC error handling for now.")]
        public void OnError_IgnoresOnCustomErrorPageIsTheOffendingPage()
        {
            // setup
            const string url = "http://myserver.com/site/errorpage";
            var x = NewHttpContext(url);
            x.AddError(new Exception("error 1"));
            //_linkGenerator.Setup(l => l.GenerateServerErrorLink()).Returns("/site/errorpage");
            var module =DependencyResolver.Current.Get<MyServerErrorHttpModule>();

            // test and sense
            module.CallOnError(x);

            Assert.False(x.Response.IsRequestBeingRedirected);
        }

        [Fact(Skip = "Using MVC error handling for now.")]
        public void OnError_RedirectsToErrorPageNoSessionService()
        {
            // setup
            const string url = "http://myserver.com/site/page?param=value";
            var x = NewHttpContext(url);
            var guid = Guid.NewGuid();
            x.Items.Add("loggingId", guid);
            const string msg = "booboo";
            Exception exc;
            try
            {
                throw new Exception(msg); // to get a stack trace
            }
            catch (Exception ex)
            {
                exc = ex;
            }
            x.AddError(exc);
            //_linkGenerator.Setup(l => l.GenerateServerErrorLink()).Returns("/site/errorpage");
            var module =DependencyResolver.Current.Get<MyServerErrorHttpModule>();

            // test
            module.CallOnError(x);

            // sense
            Assert.True(x.Response.IsRequestBeingRedirected);
            Assert.Equal("/site/errorpage", x.Response.RedirectLocation);
            var cookie = x.Response.Cookies[ServerErrorHttpModule.ServerErrorCookieName];
            Assert.NotNull(cookie);
            string id, uri, ms, trace;
            ServerErrorHttpModule.DecodeError(cookie.Value, out id, out uri, out ms, out trace);
            Assert.Equal(guid.ToString(), id);
            Assert.Equal(uri, url);
            Assert.Equal(msg, ms);
            Assert.Contains("OnError_RedirectsToErrorPage", trace);
        }
        #endregion

        #region internal

        private static HttpContext NewHttpContext(string url)
        {
            var i = url.IndexOf("?", StringComparison.Ordinal);
            var q = string.Empty;
            if (i > 0)
                q = url.Substring(i + 1);
            return new HttpContext(new HttpRequest("file", url, q)
                {
                    Browser = new HttpBrowserCapabilities
                        {
                            Capabilities = new Dictionary<string, string> {{"requiresPostRedirectionHandling", "false"}}
                        }
                }, new HttpResponse(TextWriter.Null));
        }


        public class MyServerErrorHttpModule : ServerErrorHttpModule
        {
            public void CallOnError(HttpContext context)
            {
                OnError(context, null);
            }
        }
        #endregion
    }
}
