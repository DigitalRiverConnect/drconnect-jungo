using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ActionFilters;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Resources;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using N2.Collections;
using N2.Web;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(SearchPage))]
    public class SearchPageController : ContentControllerBase<SearchPage>
    {
        private readonly IPageInfo _pageInfo;

        public SearchPageController(IRequestLogger logger, ILinkGenerator linkGenerator, IPageInfo pageInfo,
            ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            _pageInfo = pageInfo;
        }

        private const string DefaultPageSize = "16";
        //
        // GET: /Search/

        public override ActionResult Index()
        {
            // make sure this is in <appSettings>
            //	  <add key="aspnet:UseTaskFriendlySynchronizationContext" value="true" />

            var filter = new TypeFilter(typeof(PageModelBase)) { Inverse = true };
            var parts = CurrentItem.GetChildren(filter);
            var productResultsPart =
                parts.SelectMany(CmsFinder.FindAllDescendentsOf<ProductResultsPart>).FirstOrDefault() ??
                parts.FirstOrDefault(p => p is ProductResultsPart) as ProductResultsPart;

            var pageHasProdResultsPart = productResultsPart != null;
            var listPriceRanges = productResultsPart != null ? productResultsPart.ListPriceRanges : string.Empty;
            var listPriceRangesNames = Regex.Split(listPriceRanges, @"\s*,\s*").ToList();

            var query = Request["query"];
            var page = HttpUtility.UrlEncode(Request["pg"]);
            /* can't use query param named "page" because N2 grabs it and makes a big nuisance of itself */
            var pageSize = HttpUtility.UrlEncode(Request["pageSize"]);
            if (string.IsNullOrEmpty(pageSize))
                pageSize = DefaultPageSize;
            var sortBy = HttpUtility.UrlEncode(Request["sortBy"]);
            var sortDir = HttpUtility.UrlEncode(Request["sortDir"]);

            // search in commerce
            var searchOptions = SearchOptionsUtils.GetPagingOptions(page, pageSize, sortBy, sortDir);

            //var facets = JsonToFacets(Request["facetsJson"]);

            var catalogPageViewModel = new SearchPageViewModel
            {
                Title = query,
                Products = CatalogApi.GetProductsByKeywordAsync(string.Format("{0}", query), searchOptions).Result
            };

#if false
            bool enableFacets = catalogPageViewModel.Products.ExtendedSearchResult != null
                && catalogPageViewModel.Products.ExtendedSearchResult.FacetFields != null 
                && catalogPageViewModel.Products.ExtendedSearchResult.FacetFields.Length > 0;

            if (enableFacets)
            {
                catalogPageViewModel.Products.PageSize = DefaultPageSize_3across.AsInt();
                catalogPageViewModel.Products.TotalPages = (catalogPageViewModel.Products.TotalCount / DefaultPageSize_3across.AsInt()) + 1;
                catalogPageViewModel.Products.Products =
                    catalogPageViewModel.Products.Products.Take(catalogPageViewModel.Products.Products.Length -
                                                                    1).ToArray();
            }
#endif

            if (catalogPageViewModel.Products.Product == null)
                catalogPageViewModel.Products.Product = new ProductWithRanking[0];


            // if facets specified, need to render page with full facets checkbox selections, but with restricted results
            // this will almost always be a cheap call, because we probably just did it moments before and cached it
#if false
            if (facets != null)
            {
                catalogPageViewModel.Facets = facets;
                var noFacetsResults = _catAdapter.SearchProduct(query, searchOptions);
                if (catalogPageViewModel.Products.ExtendedSearchResult == null)
                    catalogPageViewModel.Products.ExtendedSearchResult = noFacetsResults.ExtendedSearchResult;
                else if (noFacetsResults.ExtendedSearchResult != null)
                    catalogPageViewModel.Products.ExtendedSearchResult.FacetFields =
                        noFacetsResults.ExtendedSearchResult.FacetFields;
                else
                    catalogPageViewModel.Products.ExtendedSearchResult = null;
            }
#endif

            catalogPageViewModel.SetPageTitle(string.Format("{0} &ldquo;{1}&rdquo;",
                Res.Catalog_CatalogSearchResultsTitle,
                Server.HtmlEncode(query ?? "")));

            WebSession.Current.Set(WebSession.SearchResultSlot, catalogPageViewModel);

            // TODO - redirect to page when there's only one exact hit
            //      - highlight results in target page, e.g. via jQuery plugin

            SetPageTitleOverrideResKey("SearchResults");

            catalogPageViewModel.PageHasProdResultsPart = pageHasProdResultsPart;
            catalogPageViewModel.EnableFacets = false;
            catalogPageViewModel.ListPriceRangeNames = listPriceRangesNames;

            SetPageInfo(query, null, "Sales.Catalog.SearchResults");
            return View(catalogPageViewModel);
        }

        protected void SetPageInfo(string query, string categoryDisplayName, string pageName)
        {
            _pageInfo.PageName = pageName;
            if (!string.IsNullOrEmpty(query))
                _pageInfo.SearchWord = query.Replace(' ', '+');
            if (!string.IsNullOrEmpty(categoryDisplayName))
                _pageInfo.Category = categoryDisplayName;
        }



    }
}
