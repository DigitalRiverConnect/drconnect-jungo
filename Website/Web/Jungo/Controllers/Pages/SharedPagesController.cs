//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  12/13/2012  HGodinez           Created
// 

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using Jungo.Api;
using Jungo.Models.ShopperApi.Catalog;
using N2;
using N2.Controllers;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
	/// <summary>
    /// This controller will handle pages deriving from PageModelBase which are not 
	/// controlled by another controller [Controls(typeof(MyPage))]. 
	/// </summary>
    [Controls(typeof(PageModelBase))]
    public class SharedPagesController : TemplatesControllerBase<PageModelBase>, INimbusContentController
	{
	    private readonly ICatalogApi _catApi;

	    public SharedPagesController(ICatalogApi catApi)
        {
            _catApi = catApi;
        }

	    public override ActionResult Index()
        {
            AssertProductsLoaded();
            ContentControllerBase<PageModelBase>.SetPageTitleSimple(CurrentItem, ViewBag);
            return base.Index();
        }

        protected void AssertProductsLoaded(long? pageProductId = null)
	    {
            HttpContext.AssertProductsLoaded(_catApi, CurrentItem, pageProductId);
        }

        public Dictionary<long, Product> Products
        {
            get { return HttpContext.GetReqScopeProducts(); }
        }

        public List<long> BogusProductIds
        {
            get { return HttpContext.GetReqScopeBogusProductIds(); }
        }
	}

    public static class ReqScopeProductsExtension
    {
        public static void AssertProductsLoaded(this HttpContextBase httpContext, ICatalogApi catalogApi, ContentItem currentItem, long? pageProductId = null)
        {
            if (catalogApi == null) return;

            var page = currentItem as PageModelBase;
            if (page == null) return;

            var pids = page.ProductIds.ToList();
            if (pageProductId != null && !pids.Contains(pageProductId.Value))
                pids.Add(pageProductId.Value);

            var products = catalogApi.GetProductsAsync(pids).Result;
            var dictOfProducts = products.ToDictionary(p => p.Id);
            httpContext.Items["Products"] = dictOfProducts;
            var bogusProductIds = pids.Where(p => !dictOfProducts.ContainsKey(p)).ToList();
            httpContext.Items["BogusProductIds"] = bogusProductIds;
        }

        public static Dictionary<long, Product> GetReqScopeProducts(this HttpContextBase httpContext)
        {
            var products = httpContext.Items["Products"];
            if (products != null)
                return products as Dictionary<long, Product>;
            return new Dictionary<long, Product>();
        }

        public static List<long> GetReqScopeBogusProductIds(this HttpContextBase httpContext)
        {
            var bogusProductIds = httpContext.Items["BogusProductIds"];
            if (bogusProductIds != null)
                return bogusProductIds as List<long>;
            return new List<long>();
        }
    }
}
