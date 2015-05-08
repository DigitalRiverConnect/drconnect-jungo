using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ActionFilters;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Resources;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using Microsoft.Scripting.Utils;
using N2.Collections;
using N2.Web;
using Omu.ValueInjecter;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof (CatalogPage))]
    public class CatalogController : ContentControllerBase<CatalogPage>
    {
        private readonly IPageInfo _pageInfo;
        private readonly ILinkGenerator _linkGenerator;
        private const string DefaultPageSize4Across = "16";
        private readonly ICategoryViewModelBuilder _catViewModelBuilder;

        public CatalogController(IRequestLogger logger, ICategoryViewModelBuilder catViewModelBuilder,
            ILinkGenerator linkGenerator,
            IPageInfo pageInfo, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            _pageInfo = pageInfo;
            _linkGenerator = linkGenerator;
            _catViewModelBuilder = catViewModelBuilder;
        }

        public override ActionResult Index()
        {
            var filter = new TypeFilter(typeof (PageModelBase)) {Inverse = true};
            var parts = CurrentItem.GetChildren(filter);
            var productResultsPart =
                parts.SelectMany(CmsFinder.FindAllDescendentsOf<ProductResultsPart>).FirstOrDefault() ??
                parts.FirstOrDefault(p => p is ProductResultsPart) as ProductResultsPart;
            var pageHasProdResultsPart = productResultsPart != null;
            var listPriceRanges = productResultsPart != null ? productResultsPart.ListPriceRanges : string.Empty;
            var listPriceRangesNames = Regex.Split(listPriceRanges, @"\s*,\s*").ToList();

            var cid = Arguments.Length > 0 ? Arguments.FirstOrDefault(a => !string.IsNullOrEmpty(a)) : null;
            if (!string.IsNullOrEmpty(cid))
            {
                ActionResult redirectResult;

                long lcid;
                if(long.TryParse(cid, out lcid))
                    if (GetRedirect(long.Parse(cid), out redirectResult)) 
                        return redirectResult;
            }

            AssertProductsLoaded();

            var page = Request["pg"];
            /* can't use query param named "page" because N2 grabs it and makes a big nuisance of itself */
            var pageSize = Request["pageSize"];
            if (string.IsNullOrEmpty(pageSize))
                pageSize = DefaultPageSize4Across;
            var sortBy = Request["sortBy"];
            var sortDir = Request["sortDir"];

            // use CMS configured category as a fallback on null, so that admins have better WYSIWYG
            if (string.IsNullOrEmpty(cid)) cid = CurrentItem.CategoryID;

            long categoryId;
            try
            {
                categoryId = long.Parse(cid);
            }
            catch
            {
                return !IsManaging ? NotFound() : View();
            }

            var searchOptions = SearchOptionsUtils.GetPagingOptions(page, pageSize, sortBy, sortDir);

            // If page is based on product results, then the count should be based on the category.
            var catalogPageViewModel =
                _catViewModelBuilder.SearchProductByCategoryAsync(categoryId, searchOptions).Result;

#if false // Facet logic
            var enableFacets = productResultsPart != null
                               && productResultsPart.EnableFacets
                               && catalogPageViewModel.Products.ExtendedSearchResult != null
                               && catalogPageViewModel.Products.ExtendedSearchResult.FacetFields != null
                               && catalogPageViewModel.Products.ExtendedSearchResult.FacetFields.Length > 0;
            if (enableFacets)
            {
                catalogPageViewModel.Products.PageSize = DefaultPageSize_3across.AsInt();
                catalogPageViewModel.Products.TotalPages = (catalogPageViewModel.Products.TotalCount/
                                                                DefaultPageSize_3across.AsInt()) + 1;
                catalogPageViewModel.Products.Products =
                    catalogPageViewModel.Products.Products.Take(catalogPageViewModel.Products.Products.Length -
                                                                    1).ToArray();
            }
#endif

            catalogPageViewModel.PageHasProdResultsPart = pageHasProdResultsPart;
            catalogPageViewModel.EnableFacets = false;
            catalogPageViewModel.ListPriceRangeNames = listPriceRangesNames;

            // if facets specified, need to render page with full facets checkbox selections, but with restricted results
            // this will almost always be a cheap call, because we probably just did it moments before and cached it
#if false // Facet logic
            if (facets != null)
            {
                catalogPageViewModel.Facets = facets;
                var noFacetsResults = _catViewModelBuilder.SearchProductByCategoryAsync(categoryId, searchOptions);
                if (catalogPageViewModel.Products.ExtendedSearchResult == null)
                    catalogPageViewModel.Products.ExtendedSearchResult =
                        noFacetsResults.Products.ExtendedSearchResult;
                else if (noFacetsResults.Products.ExtendedSearchResult != null)
                    catalogPageViewModel.Products.ExtendedSearchResult.FacetFields =
                        noFacetsResults.Products.ExtendedSearchResult.FacetFields;
                else
                    catalogPageViewModel.Products.ExtendedSearchResult = null;
            }
#endif

            if (!pageHasProdResultsPart && catalogPageViewModel.Products.Product.Length == 0)
            {
                var products = new List<Product>();

                if (Products.Any())
                    products.AddRange(Products.Values.ToArray());
                
                catalogPageViewModel.Products.Product.AddRange(
                    products.Select(p => new ProductWithRanking().InjectFrom(p) as ProductWithRanking));
            }

            if (catalogPageViewModel.Products.Product == null)
                catalogPageViewModel.Products.Product = new ProductWithRanking[0];

            WebSession.Current.Set(WebSession.SearchResultSlot, catalogPageViewModel);
            WebSession.Current.Set(WebSession.CategoryIdSlot, categoryId);

            SetPageTitle(GetPageTitle(catalogPageViewModel));
            SetPageMetaData(catalogPageViewModel);

            catalogPageViewModel.Metadata.OgType = !string.IsNullOrEmpty(CurrentItem.OgType) ? CurrentItem.OgType : "website";

            SetPageInfo(null, catalogPageViewModel.Title, "Sales.Catalog.Product");
            return View(catalogPageViewModel);
        }

        private const string CategoryNameSubstititionToken = "{CaTEGoryName}";

        protected string GetPageTitle(CatalogPageViewModel model)
        {
            var title = GetPageTitleFromCurrentItem();
            if (!string.IsNullOrEmpty(title))
                return title;

            if (model != null && !string.IsNullOrEmpty(model.Title))
            {
                var res = Res.PageTitle_Category;
                if (!string.IsNullOrEmpty(res))
                {
                    var idx = res.IndexOf(CategoryNameSubstititionToken, StringComparison.InvariantCultureIgnoreCase);
                    return idx >= 0
                        ? res.Remove(idx, CategoryNameSubstititionToken.Length).Insert(idx, model.Title)
                        : res;
                }
            }

            return GetPageTitleFromResource();
        }

        protected bool GetRedirect(long categoryId, out ActionResult redirectResult)
        {
            redirectResult = null;
            if (GetRedirectFromConfig(categoryId, ref redirectResult)) return true;
            if (GetRedirectFromSite(categoryId, ref redirectResult)) return true;
            return false;
        }

        // Redirect if there's a specific page created for this category.
        protected bool GetRedirectFromSite(long categoryId, ref ActionResult redirectResult)
        {
            if (string.IsNullOrEmpty(CurrentItem.CategoryID))
            {
                var categorySpecificUrl = LinkGenerator.GenerateCategoryLink(categoryId);
                var genericCategoryUrl = LinkGenerator.GenerateCategoryLink();
                var doNotRedirect = !string.IsNullOrEmpty(Request["list"]);
                if (!categorySpecificUrl.StartsWith(genericCategoryUrl, StringComparison.InvariantCultureIgnoreCase) &&
                    !doNotRedirect)
                {
                    redirectResult = RedirectPermanent(this.AssureHttpUrl(categorySpecificUrl));
                    return true;
                }
            }
            return false;
        }

        // redirect if config says there's a switcheroo
        protected bool GetRedirectFromConfig(long categoryId, ref ActionResult redirectResult)
        {
            var catRed = CategoryRedirectConfig.GetCategoryRedirect(categoryId);
            if (catRed != null)
            {
                switch (catRed.ToWhat)
                {
                    case CategoryRedirectToWhat.CategoryId:
                        if (catRed.Value != categoryId.ToString(CultureInfo.InvariantCulture))
                            // don't do infinite redirect loop infinite redirect loop infinite redirect loop infinite redirect ...
                        {
                            redirectResult =
                                RedirectPermanent(
                                    this.AssureHttpUrl(LinkGenerator.GenerateCategoryLink(long.Parse(catRed.Value))));
                            return true;
                        }
                        break;
                    case CategoryRedirectToWhat.ProductId:
                        long pid;
                        redirectResult =
                            RedirectPermanent(this.AssureHttpUrl(LinkGenerator.GenerateProductLink(long.TryParse(catRed.Value, out pid) ? pid : (long?)null)));
                        return true;
                    case CategoryRedirectToWhat.Path:
                        // gen url based on store link minus last segment for store itself
                        var storeLink = _linkGenerator.GenerateStoreLink();
                        if (!string.IsNullOrEmpty(storeLink))
                        {
                            var lastSlashIdx = storeLink.LastIndexOf('/');
                            if (lastSlashIdx >= 0)
                            {
                                var redirectUrl = storeLink.Substring(0, lastSlashIdx);
                                if (!catRed.Value.StartsWith("/"))
                                    redirectUrl += "/";
                                redirectUrl += catRed.Value;
                                redirectResult = RedirectPermanent(this.AssureHttpUrl(redirectUrl));
                                return true;
                            }
                        }
                        break;
                }
            }
            return false;
        }

        // the following needs to be in a controller not requiring https and not requiring shopper authorization
        // because it could get called in a non-https mode and without a shopper in context, such as Add-To-Cart on a product page
        [DynamicActionResult]
        [OutputCache(Duration = 0)]
        public ActionResult GetAntiForgeryToken()
        {
            return PartialView("AntiForgeryToken");
        }

        protected void SetPageInfo(string query, string categoryDisplayName, string pageName)
        {
            _pageInfo.PageName = pageName;
            if (!String.IsNullOrEmpty(query))
                _pageInfo.SearchWord = query.Replace(' ', '+');
            if (!string.IsNullOrEmpty(categoryDisplayName))
                _pageInfo.Category = categoryDisplayName;
        }

    }
}