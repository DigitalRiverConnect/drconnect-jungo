using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class CatalogPageViewModel : PageViewModelBase
    {
        public string Title { get; set; }
        public long CategoryId { get; set; }

        public bool PageHasProdResultsPart { get; set; }
        public bool EnableFacets { get; set; }
        public List<string> ListPriceRangeNames { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public Products Products { get; set; }

        // "Products" above contains full set of facets for the category or keyword
        // but "Facets" below contains facets applied when doing the search
        //   only the FacetSearchField.AttributeName and FacetConstraint.ConstraintQuery properties will have values
        //public FacetSearchField[] Facets { get; set; }
    }
}
