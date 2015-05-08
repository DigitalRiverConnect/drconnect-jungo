using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class ComparisonTableRowViewModel
    {
        public ComparisonTableRowViewModel(string title, string image, string targetUrl, string urlSuffix, string target, string linkText, Product productDetails, string cells)
        {
            Title = title;
            Image = image;
            TargetUrl = targetUrl;
            UrlSuffix = urlSuffix;
            Target = target;
            LinkText = linkText;
            ProductDetails = productDetails;
            Cells = cells;
        }

        public string Title { get; set; }
        public string LinkText { get; set; }
        public string TargetUrl { get; set; }
        public string UrlSuffix { get; set; }
        public string Target { get; set; }
        public string Image { get; set; }
        public Product ProductDetails { get; set; }
        public string Cells { get; set; }
    }

    public class ComparisonTableViewModel : PageViewModelBase
    {
        public string Title { get; set; }
        public string Columns { get; set; }
        public IList<ComparisonTableRowViewModel> Rows { get; set; }
    }
}
