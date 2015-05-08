using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class CategoryListViewModel
    {
        public CategoryListViewModel()
        {
            Categories = new List<CategoryListItemViewModel>();
        }
        public string Title { get; set; }
        public IList<CategoryListItemViewModel> Categories { get; private set; }
    }

    public class CategoryListItemViewModel
    {
        public Category Category { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
