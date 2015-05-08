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
//  3/25/2012   EHornbostel        Works with API
// 

using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Resources;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Extensions;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using ViewModelBuilders.Catalog;
using Pricing = Jungo.Models.ShopperApi.Catalog.Pricing;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(ProductPage))]
    public class ProductController : ContentControllerBase<ProductPage>
    {
        private readonly IPageInfo _pageInfo;
        private readonly IProductViewModelBuilder _prodViewModelBuilder;
        private readonly ICatalogApi _catApi;

        public ProductController(ICatalogApi catApi, IProductViewModelBuilder prodViewModelBuilder, IRequestLogger logger, ILinkGenerator linkGenerator, IPageInfo pageInfo)
            : base(logger, linkGenerator, catApi)
        {
            _pageInfo = pageInfo;
            _prodViewModelBuilder = prodViewModelBuilder;
            _catApi = catApi;
        }

        public override ActionResult Index()
        {
            var productId = Arguments.Length > 0
                ? Arguments[0]
                : (CurrentItem == null || string.IsNullOrEmpty(CurrentItem.ProductID)
                    ? null
                    : CurrentItem.ProductID.Split().First());

            long? pid;

            if (string.IsNullOrEmpty(productId))
                pid = null;
            else
            {
                long tempId;
                pid = long.TryParse(productId, out tempId) ? tempId : (long?)null;
            }

            AssertProductsLoaded(pid);

            if (pid != null && BogusProductIds.Contains(pid.Value))
                productId = null;

            // Redirect if there's a specific page created for this product.
            if (CurrentItem != null && string.IsNullOrEmpty(CurrentItem.ProductID) && pid != null)
            {
                var prodSpecificUrl = LinkGenerator.GenerateProductLink(pid);
                var productUrl = LinkGenerator.GenerateProductLink();
                if (!prodSpecificUrl.StartsWith(productUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    return RedirectPermanent(this.AssureHttpUrl(prodSpecificUrl + Request.Url.Query));
                }
            }


            ProductDetailPageViewModel model = null;
            if (pid != null)
            {
                SiteInfo si;
                WebSession.Current.TryGetSiteInfo(out si);
                Product p;
                if (Products.TryGetValue(pid.Value, out p))
                {
                    model = _prodViewModelBuilder.GetProductDetail(p, si, false).Result;
                }
                else if (IsManaging)
                {
                    model = GetDemoProduct();
                }
            }

            if (model == null && !IsManaging)
                return NotFound();
            if (model == null)
                model = GetDemoProduct();

#if SAVE_VIEW_MODEL
            SaveViewModel(model, model.Id);
#endif
            SetPageTitle(GetPageTitle(model));
            SetPageMetaData(model);

            WebSession.Current.Set(WebSession.CurrentProductSlot, model);
            WebSession.Current.SetSessionAfterAddToCartOption(CurrentItem.AfterAddToCartOption);

            SetPageInfo(_pageInfo, model, "Sales.Product");
            return View("PageTemplates/Default", model);
        }

        private ProductDetailPageViewModel GetDemoProduct()
        {
            SiteInfo si;
            WebSession.Current.TryGetSiteInfo(out si);
            var prod = new Product
            {
                Pricing = new Pricing(),
                DisplayName = "demo display name",
                ShortDescription = "demo",
                InventoryStatus = new InventoryStatus()
            };
            var ret = new ProductDetailPageViewModel(si) {Product = prod, DisplayProduct = prod};

            return ret;
        }

        public static void SetPageInfo(IPageInfo pageInfo, ProductDetailPageViewModel model, string pageName)
        {
            pageInfo.PageName = pageName;
            if (model == null)
                pageInfo.ProductStatus = "unavailable";
            else
            {
                if (!model.Product.HasVariations() || model.Product.Variations == null)
                {
                    pageInfo.ProductName = model.Product.DisplayName;
                    if (model.Product.Pricing.ListPrice != null)
                        pageInfo.ProductPrice = model.Product.Pricing.ListPrice.Value;
                }
                else
                {
                    var productVariation = model.Product.Variations.Product.FirstOrDefault();
                    if (productVariation != null)
                    {
                        pageInfo.ProductName = model.Product.DisplayName;
                        if (model.Product.Pricing.ListPrice != null)
                            pageInfo.ProductPrice = productVariation.Pricing.ListPrice.Value;
                    }
                }
            }
        }

        // These should be obfuscated in at least 18 levels of indirection, configuration and dependency injection.
        // But for now, here they are.
        // Use proper eye protection when looking directly at them to avoid permanent retina damage.
        private const string SuperDuperTitleTagAttributeName = "Custom Title";
        private const string ProductNameSubstititionToken = "{ProductName}";

        public string GetPageTitleFromModel(ProductDetailPageViewModel model)
        {
            if (model != null)
            {
                var title = model.Product.CustomAttributes.ValueByName(SuperDuperTitleTagAttributeName);
                if (!string.IsNullOrEmpty(title))
                    return title;
                if (model.ParentProductId != 0 && model.ParentProductId != model.Product.Id)
                {
                    var parentProd = _catApi.GetProductAsync(_catApi.GetProductUri(model.ParentProductId)).Result;

                    title = parentProd.CustomAttributes.ValueByName(SuperDuperTitleTagAttributeName);
                    if (!string.IsNullOrEmpty(title))
                        return title;
                }
                var res = Res.PageTitle_Product;
                if (!string.IsNullOrEmpty(res))
                {
                    var idx = res.IndexOf(ProductNameSubstititionToken, StringComparison.InvariantCultureIgnoreCase);
                    if (idx >= 0)
                    {
                        string name;
                        if (!string.IsNullOrEmpty(model.Product.DisplayName))
                            name = model.Product.DisplayName;
                        else
                            name = model.Product.Id.ToString(CultureInfo.InvariantCulture);
                        return res.Remove(idx, ProductNameSubstititionToken.Length).Insert(idx, name);
                    }
                    return res;
                }
            }
            return string.Empty;
        }

        protected string GetPageTitle(ProductDetailPageViewModel model)
        {
            var title = GetPageTitleFromCurrentItem();
            if (!string.IsNullOrEmpty(title))
                return title;
            title = GetPageTitleFromModel(model);
            return !string.IsNullOrEmpty(title) ? title : GetPageTitleFromResource();
        }
    }
}
