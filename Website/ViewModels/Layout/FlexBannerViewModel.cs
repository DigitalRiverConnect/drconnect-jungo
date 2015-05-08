using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class FlexBannerItemViewModel
    {
        public FlexBannerItemViewModel(string title, string promoText, string imageDesktop, string imageTablet, string imageMobile, string targetUrl, string urlSuffix, string target, string linkText, string htmlContent, int itemWidth)
        {
            Title = title;
            PromoText = promoText;
            ImageDesktop = imageDesktop;
            ImageTablet = imageTablet;
            ImageMobile = imageMobile;
            TargetUrl = targetUrl;
            UrlSuffix = urlSuffix;
            Target = target;
            LinkText = linkText;
            HtmlContent = htmlContent;
            ItemWidth = itemWidth;
        }

        public string Title { get; set; }
        public string LinkText { get; set; }
        public string TargetUrl { get; set; }
        public string UrlSuffix { get; set; }
        public string Target { get; set; }
        public string PromoText { get; set; }
        public string ImageDesktop { get; set; }
        public string ImageTablet { get; set; }
        public string ImageMobile { get; set; }
        public string HtmlContent { get; set; }
        public int ItemWidth { get; set; }
    }

    public class FlexBannerCategoryItemViewModel : FlexBannerItemViewModel
    {
        public FlexBannerCategoryItemViewModel(string title, string promoText, string imageDesktop, string imageTablet, string imageMobile, string targetUrl, string urlSuffix, string target, string linkText, string htmlContent, int itemWidth, string categoryId, bool forceListPage, string bgColor)
            : base(title, promoText, imageDesktop, imageTablet, imageMobile, targetUrl, urlSuffix, target, linkText, htmlContent, itemWidth)
        {
            CategoryId = categoryId;
            ForceListPage = forceListPage;
            BgColor = bgColor;
        }

        public string CategoryId { get; set; }
        public bool ForceListPage { get; set; }
        public string BgColor { get; set; }
    }

    public class FlexBannerOneColumnBannerItemViewModel : FlexBannerItemViewModel
    {
        public FlexBannerOneColumnBannerItemViewModel(string title, string promoText, string imageDesktop, string imageTablet, string imageMobile, string targetUrl, string urlSuffix, string target, string linkText, string htmlContent, int itemWidth, string backgroundColor, string foregroundColor)
            : base(title, promoText, imageDesktop, imageTablet, imageMobile, targetUrl, urlSuffix, target, linkText, htmlContent, itemWidth)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
        }

        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
    }

    public class FlexBannerProductItemViewModel : FlexBannerItemViewModel
    {
        public FlexBannerProductItemViewModel(string title, string promoText, string imageDesktop, string imageTablet, string imageMobile, string targetUrl, string urlSuffix, string target, string linkText, string htmlContent, int itemWidth, string badgeText, Product productDetails)
            : base(title, promoText, imageDesktop, imageTablet, imageMobile, targetUrl, urlSuffix, target, linkText, htmlContent, itemWidth)
        {
            ProductDetails = productDetails;
            BadgeText = badgeText;
        }

        public string BadgeText { get; set; }
        public Product ProductDetails { get; set; }
    }

    public class FlexBannerTwoColumnBannerItemViewModel : FlexBannerItemViewModel
    {
        public FlexBannerTwoColumnBannerItemViewModel(string title, string promoText, string imageDesktop, string imageTablet, string imageMobile, string targetUrl, string urlSuffix, string target, string linkText, string htmlContent, int itemWidth, string backgroundColor, string foregroundColor)
            : base(title, promoText, imageDesktop, imageTablet, imageMobile, targetUrl, urlSuffix, target, linkText, htmlContent, itemWidth)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
        }

        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
    }

    public class FlexBannerViewModel : PageViewModelBase
    {
        public string Title { get; set; }
        public IList<FlexBannerItemViewModel> Tiles { get; set; }
    }
}
