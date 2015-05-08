using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class PromoTilesItemViewModel
    {
        public PromoTilesItemViewModel(string title, string subtitle, string description, string imageDesktop, string imageTablet, string imageMobile,
            string linkText, string targetUrl, string urlSuffix, string target, Product productDetails)
        {
            Title = title;
            Subtitle = subtitle;
            Description = description;
            ImageDesktop = imageDesktop;
            ImageTablet = imageTablet;
            ImageMobile = imageMobile;
            LinkText = linkText;
            TargetUrl = targetUrl;
            UrlSuffix = urlSuffix;
            Target = target;
            ProductDetails = productDetails;
        }

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string ImageDesktop { get; set; }
        public string ImageTablet { get; set; }
        public string ImageMobile { get; set; }
        public string LinkText { get; set; }
        public string TargetUrl { get; set; }
        public string UrlSuffix { get; set; }
        public string Target { get; set; }
        public Product ProductDetails { get; set; }
    }

    public class PromoTilesViewModel : PageViewModelBase
    {
        public IList<PromoTilesItemViewModel> Tiles { get; set; }
    }
}
