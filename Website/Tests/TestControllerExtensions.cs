using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests
{
	public static class TestControllerExtensions
	{
		/// <summary>
		/// allows access to things such as the controller's HttpContext, HttpContext.Request, HttpContext.Request.QueryString, HttpContext.Request.Cookies
		/// </summary>
		public static void SetMockControllerContext(this Controller yourController, HttpContextTestData httpContextTestData = null)
		{
			var request = new Mock<HttpRequestBase>();
			var response = new Mock<HttpResponseBase>();
            var requestQuery = httpContextTestData == null ? new NameValueCollection() : httpContextTestData.RequestQueryParams;
		    var reqCookies = new HttpCookieCollection();
		    var respCookies = new HttpCookieCollection();
            // controller methods might access query params either as Request.QueryString or as Request["xxx"]
            //   make Request.QueryString work:
			request.SetupGet(x => x.QueryString).Returns(requestQuery);
            //   make Request["xxx"] work:
            if (requestQuery.Count > 0)
                request.SetupGet(r => r[requestQuery.AllKeys[0]]).Returns(requestQuery.Get(0));
            if (requestQuery.Count > 1)
                request.SetupGet(r => r[requestQuery.AllKeys[1]]).Returns(requestQuery.Get(1));
            if (requestQuery.Count > 2)
                request.SetupGet(r => r[requestQuery.AllKeys[2]]).Returns(requestQuery.Get(2));
            if (requestQuery.Count > 3)
                request.SetupGet(r => r[requestQuery.AllKeys[3]]).Returns(requestQuery.Get(3));
            if (requestQuery.Count > 4)
                request.SetupGet(r => r[requestQuery.AllKeys[4]]).Returns(requestQuery.Get(4));
            request.SetupGet(x => x.Cookies).Returns(reqCookies);
		    if (httpContextTestData != null && !string.IsNullOrEmpty(httpContextTestData.Url))
		        request.SetupGet(x => x.Url).Returns(new Uri(httpContextTestData.Url));
            response.SetupGet(x => x.Cookies).Returns(respCookies);
            var context = new Mock<HttpContextBase>();
			context.SetupGet(x => x.Request).Returns(request.Object);
		    context.SetupGet(x => x.Response).Returns(response.Object);
            if (httpContextTestData != null)
                response.Setup(x => x.Redirect(It.IsAny<string>())).Callback<string>(url => httpContextTestData.RedirectUrl = url);
            yourController.ControllerContext = new ControllerContext(context.Object, new RouteData(), yourController);
		}
	}

    public class HttpContextTestData
    {
        private string _url = string.Empty;

        /// <summary>
        /// set the url, which also has the effect of adding query params automatically for you
        /// </summary>
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                var uri = new Uri(_url);
                var q = uri.Query;
                if (q.StartsWith("?"))
                    q = q.Remove(0, 1);
                foreach (var nv in q.Split('&').Select(p => p.Split('=')))
                    AddRequestQueryParam(nv[0], nv.Length > 1 ? nv[1] : string.Empty);
            }
        }

        // gets set if Response.Redirect called
        public string RedirectUrl { get; set; }

        private readonly NameValueCollection _requestQueryParams = new NameValueCollection();

        public NameValueCollection RequestQueryParams
        {
            get { return _requestQueryParams; }
        }

        /// <summary>
        /// add query params here if all you care about are the params; otherwise, just set the url and query params will automatically be added for you
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddRequestQueryParam(string name, string value)
        {
            RequestQueryParams.Add(name, value);
        }
    }
}
