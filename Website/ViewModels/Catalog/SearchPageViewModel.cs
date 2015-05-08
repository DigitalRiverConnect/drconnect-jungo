using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class SearchPageViewModel : PageViewModelBase
    {
        public string Title { get; set; }

        public bool PageHasProdResultsPart { get; set; }
        public bool EnableFacets { get; set; }
        public List<string> ListPriceRangeNames { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public ProductsWithRanking Products { get; set; }
    }
}
