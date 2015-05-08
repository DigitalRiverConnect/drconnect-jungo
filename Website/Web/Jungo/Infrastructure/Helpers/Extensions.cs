//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
// 

using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Utils;
using Jungo.Api;
using Jungo.Models.ShopperApi.Catalog;
using N2.Engine;
using N2.Interfaces;
using N2.Web;
using N2.Web.Mvc;
using N2.Web.Mvc.Html;
using N2.Web.UI.WebControls;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers
{
    public static class Extensions
    {
        private static ILinkGenerator _linkGen;
        public static ILinkGenerator LinkGenerator
        {
            get { return _linkGen ?? (_linkGen = N2.Context.Current.Resolve<ILinkGenerator>()); }
        }

        private static ICatalogApi _catAdapter;
        public static ICatalogApi CatalogApi
        {
            get { return _catAdapter ?? (_catAdapter = N2.Context.Current.Resolve<ICatalogApi>()); }
        }

        /// <summary>
        /// Helper to create a valid CMS link for offers from GC
        /// requires that TargetUrl can be parsed or categoryID is set
        /// </summary>
        /// <param name="ovm"></param>
        /// <returns></returns>
        public static string CmsLink(this OfferViewModel ovm)
        {
            // create category links
            if (!string.IsNullOrEmpty(ovm.CategoryId))
                return LinkGenerator.GenerateCategoryLink(long.Parse(ovm.CategoryId));

            if (!string.IsNullOrEmpty(ovm.TargetUrl))
            {
                // external links are passed thru
                if (ovm.TargetUrl.StartsWith("http"))
                    return ovm.TargetUrl;

                if (string.IsNullOrEmpty(ovm.CategoryId) && !string.IsNullOrEmpty(ovm.TargetUrl))
                {
                    // TODO detect valid cms link and skip
                    // need to parse GC link
                    ovm.CategoryId = ovm.TargetUrl.ExtractUrlId("categoryID");
                }

                // TODO in Dev Mode only
                if (!string.IsNullOrEmpty(ovm.OfferId))
                    return "https://gc.digitalriver.com/gc/ent/merchandising/offer/offerDetails.do?offerID=" +
                           ovm.OfferId;

                if (!string.IsNullOrEmpty(ovm.TargetUrl))
                    return ovm.TargetUrl;
            }

            return null;
        }

        public static string CmsLink(this Product product)
        {
            // create product link
            return ProductDetailLink(null, product.Id);
        }


        /// <summary>
        /// Extract ID from GC style Urls, e.g.
        ///  /store/msstore/html/pbpage.Windows_8_Pro?Icid=Home_4up_Win8
        ///  /store/msstore/cat/categoryID.37286600?Icid=Home_4up_1_OfficeCatPage
        /// </summary>
        /// <param name="value">url input</param>
        /// <param name="name">name/indentifier to be extracted, e.g. "categoryID"</param>
        /// <returns>extracted id</returns>
        public static string ExtractUrlId(this string value, string name)
        {
            var p = value.IndexOf(name, StringComparison.InvariantCultureIgnoreCase);
            if (p > 0)
            {
                var s = value.Substring(p + name.Length + 1); // add 1 for . or =
                var e = s.IndexOfAny(new [] { '?', '&' }); // terminator
                return e > 0 ? s.Substring(0, e) : s;
            }
            return null;
        }

        /// <summary>
        /// determines if user is actively organizing parts / editing the page interactively via drag & drop
        /// </summary>
        /// <param name="html"></param>
        /// <returns>true if managing</returns>
        public static bool IsOrganizing(this HtmlHelper html)
        {
            var engine = html.ContentEngine();

            return engine != null && ControlPanel.GetState(engine).IsFlagSet(ControlPanelState.DragDrop);
        }

        /// <summary>
        /// determines if user is actively managing (administering) the site vs. using the site
        /// </summary>
        /// <param name="html"></param>
        /// <returns>true if managing</returns>
        public static bool IsManaging(this HtmlHelper html)
        {
            return IsManaging(html.ContentEngine());
        }

        /// <summary>
        /// determines if user is actively managing (administering) the site vs. using the site
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns>true if managing</returns>
        public static bool IsManaging(this ContentController ctrl)
        {
            return IsManaging(ctrl.Engine);
        }

        private const string ViewPreferenceQueryString = "view"; // this const is declared in N2 source, but is not accessible
        private const string EditQueryString = "edit"; // this is hard-coded all over the place in N2 source

        /// <summary>
        /// determines if user is actively managing (administering) the site vs. using the site
        /// </summary>
        /// <param name="engine"></param>
        /// <returns>true if managing</returns>
        public static bool IsManaging(this IEngine engine)
        {
            if (engine == null) // no engine, can't possibly be managing
                return false;
            if (ControlPanel.GetState(engine).IsManaging()) // a first, easy test, but not all inclusive
                return true;
            // we might not be "managing" according to N2's narrower definition,
            //   but if we're sitting in the control panel or in "organize parts" mode outside the panel,
            //   we consider this "managing" even though N2 might not
            // in the end, N2's ControlPanel.GetState() actually looks at the url query param to see if N2 is in admin mode;
            //   it just doesn't go far enough for us;
            if (engine.RequestContext.HttpContext != null &&
                engine.RequestContext.HttpContext.Request != null &&
                engine.RequestContext.HttpContext.Request.UrlReferrer != null)
            {
                var managementUrl = new Url(((HttpContext)null).GetManagementUrl());
                if (engine.RequestContext.HttpContext.Request.UrlReferrer.LocalPath.Contains(managementUrl.ApplicationRelativePath))
                    return true;
            }

            var urlParams = engine.RequestContext.Url.GetQueries().ToNameValueCollection();
            return
                !string.IsNullOrEmpty(urlParams[ViewPreferenceQueryString]) ||
                !string.IsNullOrEmpty(urlParams[EditQueryString]) || 
                (engine.RequestContext.User != null && engine.RequestContext.User.Identity.IsAuthenticated);
        }

        /*~~
         public static bool IsEditMode(IPrincipal user)
         {
             var managementUrl = new N2.Web.Url(System.Web.HttpContext.Current.GetManagementUrl());
             if (user.IsInRole("Administrators") && (
                  (Request.UrlReferrer != null && Request.UrlReferrer.LocalPath.Contains(managementUrl.ApplicationRelativePath))
                  || (Request["edit"] != null && Request["edit"].Equals("drag", StringComparison.InvariantCultureIgnoreCase))
                  || (Request["edit"] != null && Request["edit"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                  || (Request["view"] != null && Request["view"].Equals("draft", StringComparison.InvariantCultureIgnoreCase))
                  || (Request["refresh"] != null && Request["refresh"].Equals("true", StringComparison.InvariantCultureIgnoreCase))))
                 return true;

             return false;
         }
          */

        public static bool IsCheckoutWorkflow(this HtmlHelper html)
        {
            var isInWorkflow = HttpContext.Current.Items["IsInCheckoutWorkflow"];
            return (isInWorkflow != null && (bool)isInWorkflow);
        }

        public static string CategoryPageLink(this HtmlHelper html, long? categoryId = null)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateCategoryLink(categoryId));
        }

        /// <summary>
        /// Create a product detail link.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="productId">The product ID.</param>
        /// <returns></returns>
        public static string ProductDetailLink(this HtmlHelper html, long? productId)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateProductLink(productId));
        }

        /// <summary>
        /// Create a product detail link for the base product.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="productId">The product ID.</param>
        /// <returns></returns>
        public static string BaseProductDetailLink(this HtmlHelper html, long productId)
        {
            var baseProductId = productId;
            try
            {
                var prod = CatalogApi.GetProductAsync(CatalogApi.GetProductUri(productId)).Result;
                if (prod != null && prod.Variations != null)
                    baseProductId = prod.Variations.Product.First().GetParentProductId();
            }
            catch (Exception)
            {
            }
            return html.AssureHttpUrl(LinkGenerator.GenerateProductLink(baseProductId));
        }

        /// <summary>
        /// Create a product detail link for a product in the shopping cart
        /// </summary>
        /// <param name="html"></param>
        /// <param name="product">A product in the shopping cart</param>
        /// <returns></returns>
        public static string BaseProductDetailLink(this HtmlHelper html, Jungo.Models.ShopperApi.Common.Product product)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateProductLink(product.Id));
        }

        /// <summary>
        /// Create a product detail link for the base product.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="product">The product</param>
        /// <returns></returns>
        public static string BaseProductDetailLink(this HtmlHelper html, Product product)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateProductLink(product.Variations == null ? product.Id : product.Variations.Product.First().GetParentProductId()));
        }

        public static string BuyLink(this HtmlHelper html, long productId, int quantity = 1)
        {
            //return null;
            //var offers = CatalogApi.GetCustomBundleOffers(productId);
            //if (offers != null && offers.Length > 0)
            //{
            //    var offerId = offers[0].Id;
            //    return LinkGenerator.GenerateBundleProductPickerLink(offerId, productId);
            //}
            var link = LinkGenerator.GenerateShoppingCartLink() ?? "";
            if (String.IsNullOrEmpty(link)) return link;
            if (!link.EndsWith("/"))
                link += "/";
            var json = "[{\"ProductId\":\"" + productId + "\",\"Quantity\":" + quantity + "}]";
            link += "addtocart?products=" + HttpUtility.UrlEncode(json);
            return link;
        }

        public static string AbsoluteStoreLink(this HtmlHelper html)
        {
            return html.AbsoluteUrlWithHttp(LinkGenerator.GenerateStoreLink());

        }
        public static string StoreLink(this HtmlHelper html)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateStoreLink());
        }

        public static string ShoppingCartLink(this HtmlHelper html)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateShoppingCartLink());
        }

        public static string SearchActionLink(this HtmlHelper html)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateSearchActionLink());
        }

        public static string ProductAddToCartLink(this HtmlHelper html, long productId)
        {
            var link = html.ShoppingCartLink();

            if (!string.IsNullOrWhiteSpace(link))
            {
                return link + "/addproduct?productId=" + productId;
            }

            return "";
        }

        public static string CheckoutLink(this HtmlHelper html, string action = "")
        {
            return String.Empty;    // here implement the link to WebCheckout!!
        }

        public static string FaqLink(this HtmlHelper html, string title = "")
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateFAQLink());
        }

        public static string ContentPageLink(this HtmlHelper html, string pageName)
        {
            return html.AssureHttpUrl(LinkGenerator.GenerateLinkForNamedContentItem(pageName));
        }

        #region Url Helpers

        public static string GetPublicURL(this HtmlHelper html, string url)
        {
            return N2.Context.Current.Resolve<IExternalWebLinkResolver>().GetPublicUrl(url);
        }

        public static string AssureHttpUrl(this HtmlHelper html, string relativeUrl)
        {
            return AssureSchemeUrl(html.ViewContext.HttpContext.Request, "http", relativeUrl);
        }

        public static string AssureHttpsUrl(this HtmlHelper html, string relativeUrl)
        {
            return AssureSchemeUrl(html.ViewContext.HttpContext.Request, "https", relativeUrl);
        }

        public static string AbsoluteUrlWithHttp(this HtmlHelper html, string relativeUrl)
        {
            return AbsoluteUrlWithScheme(html.ViewContext.HttpContext.Request, "http", relativeUrl);
        }

        public static string AbsoluteUrlWithHttps(this HtmlHelper html, string relativeUrl)
        {
            return AbsoluteUrlWithScheme(html.ViewContext.HttpContext.Request, "https", relativeUrl);
        }

        public static string AssureHttpUrl(this Controller controller, string relativeUrl)
        {
            return AssureSchemeUrl(controller.HttpContext.Request, "http", relativeUrl);
        }

        public static string AssureHttpsUrl(this Controller controller, string relativeUrl)
        {
            return AssureSchemeUrl(controller.HttpContext.Request, "https", relativeUrl);
        }

        public static string AbsoluteUrlWithHttp(this Controller controller, string relativeUrl)
        {
            return AbsoluteUrlWithScheme(controller.HttpContext.Request, "http", relativeUrl);
        }

        public static string AbsoluteUrlWithHttp(this HttpContextBase context, string relativeUrl)
        {
            return AbsoluteUrlWithScheme(context.Request, "http", relativeUrl);
        }

        public static string AbsoluteUrlWithHttps(this Controller controller, string relativeUrl)
        {
            return AbsoluteUrlWithScheme(controller.HttpContext.Request, "https", relativeUrl);
        }

        // public for testing only
        public static string AssureSchemeUrl(HttpRequestBase request, string targetScheme,
                                             string relativeUrl)
        {
            if (request.Url.Scheme.ToLower() == targetScheme.ToLower() || IsManaging(N2.Context.Current))
                return relativeUrl;
            // If we must change scheme, the url must become an absolute url, so ...
            return AbsoluteUrlWithScheme(request, targetScheme, relativeUrl);
        }

        // public for testing only
        public static string AbsoluteUrlWithScheme(HttpRequestBase request, string targetScheme,
                                             string relativeUrl)
        {
            return AbsoluteUrlWithScheme(request.Url.Authority, targetScheme, relativeUrl);
        }

        public static string AbsoluteUrlWithScheme(string authority, string targetScheme, string relativeUrl)
        {
            if (relativeUrl == null)
                return null;
            if (!relativeUrl.StartsWith("/"))
                relativeUrl = "/" + relativeUrl;
            return targetScheme + "://" + authority + relativeUrl;
        }

        #endregion
    }
}