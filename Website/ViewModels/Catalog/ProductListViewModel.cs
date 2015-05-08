using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class ProductListViewModel
    {
        public ProductListViewModel()
        {
            Products = new List<ProductListItemViewModel>();
        }
        public string Title { get; set; }
        public IList<ProductListItemViewModel> Products { get; private set; }
    }

    public class ProductListItemViewModel
    {
        public Product Product { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
