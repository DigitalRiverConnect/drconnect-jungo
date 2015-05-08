using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class CategoryNavigationItemViewModel
    {
        public CategoryNavigationItemViewModel(string title, string targetUrl, string urlSuffix, string target, Product productDetails, string categoryId)
        {
            Title = title;
            TargetUrl = targetUrl;
            UrlSuffix = urlSuffix;
            Target = target;
            ProductDetails = productDetails;
            CategoryId = categoryId;
        }

        public string Title { get; set; }
        public string TargetUrl { get; set; }
        public string UrlSuffix { get; set; }
        public string Target { get; set; }
        public Product ProductDetails { get; set; }
        public string CategoryId { get; set; }
    }

    public class CategoryNavigationViewModel : PageViewModelBase
    {
        public string Title { get; set; }
        public string NavStyle { get; set; }
        public IList<CategoryNavigationItemViewModel> Links { get; set; }
    }
}
