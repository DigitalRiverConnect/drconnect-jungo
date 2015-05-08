using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using Pricing = Jungo.Models.ShopperApi.Catalog.Pricing;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(ProductResultsPart))]
    public class ProductResultsController : PartControllerBase<ProductResultsPart>
    {
        public ProductResultsController(IRequestLogger logger)
            : base(logger)
        {

        }

        public override ActionResult Index()
        {
            var searchResult = WebSession.Current.Get<CatalogPageViewModel>(WebSession.SearchResultSlot);
            if (searchResult == null && IsManaging())
                searchResult = GetDemoSearchResult();
            var vm = new ProductResultsViewModel
            {
                EnableFacets = CurrentItem.EnableFacets,
                ListPriceRanges = CurrentItem.ListPriceRanges,
                SearchResult = searchResult
            };
            return PartialView(vm);
        }

        private static CatalogPageViewModel GetDemoSearchResult()
        {
            var ret = new CatalogPageViewModel
            {
                CategoryId = 1,
                CurrentPage = 1,
                EnableFacets = false,
                PageHasProdResultsPart = false,
                PageSize = 20,
                TotalResults = 50,
                Products = new Products()
            };

            var product = new Product[20];
            for (var i = 0; i < 20; i++)
            {
                product[i] = NewProduct();
            }

            ret.Products.Product = product;

            return ret;
        }

        private static Product NewProduct()
        {
            return new Product
            {
                DisplayName = "Lorem Ipsum",
                LongDescription =
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam commodo diam ut purus aliquam, sit amet sollicitudin nisl posuere.",
                ThumbnailImage = "http://placehold.it/150x150",
                Pricing = new Pricing
                {
                    TotalDiscountWithQuantity = new MoneyAmount {Currency = "$", Value = 2},
                    FormattedSalePriceWithQuantity = "$5.00"
                }
            };
        }
    }
}