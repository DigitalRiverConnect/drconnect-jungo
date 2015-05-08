using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class WideBannerItemViewModel
    {
        public WideBannerItemViewModel(string title, string titleImage, string heroImageDesktop, string heroImageTablet, string heroImageMobile, string subtitle, string bannerStyle, string htmlContent)
        {
            Title = title;
            TitleImage = titleImage;
            HeroImageDesktop = heroImageDesktop;
            HeroImageTablet = heroImageTablet;
            HeroImageMobile = heroImageMobile;
            Subtitle = subtitle;
            BannerStyle = bannerStyle;
            HtmlContent = htmlContent;
        }

        public string Title { get; set; }
        public string TitleImage { get; set; }
        public string HeroImageDesktop { get; set; }
        public string HeroImageTablet { get; set; }
        public string HeroImageMobile { get; set; }
        public string Subtitle { get; set; }
        public string BannerStyle { get; set; }
        public string HtmlContent { get; set; }
    }

    public class WideBannerRichHeroBannerItemViewModel : WideBannerItemViewModel
    {
        public WideBannerRichHeroBannerItemViewModel(string title, string titleImage, string heroImageDesktop, string heroImageTablet, string heroImageMobile, string subtitle, string bannerStyle, string htmlContent, string ctaStle, string linkText, string targetUrl, string urlSuffix, string target, string legalText, string videoSources, string videoImage)
            : base(title, titleImage, heroImageDesktop, heroImageTablet, heroImageMobile, subtitle, bannerStyle, htmlContent)
        {
            CtaStyle = ctaStle;
            LinkText = linkText;
            TargetUrl = targetUrl;
            UrlSuffix = urlSuffix;
            Target = target;
            LegalText = legalText;
            VideoSources = videoSources;
            VideoImage = videoImage;
        }

        public string CtaStyle { get; set; }
        public string LinkText { get; set; }
        public string TargetUrl { get; set; }
        public string UrlSuffix { get; set; }
        public string Target { get; set; }
        public string LegalText { get; set; }
        public string VideoSources { get; set; }
        public string VideoImage { get; set; }
    }

    public class WideBannerSkinnyHeaderBannerItemViewModel : WideBannerItemViewModel
    {
        public WideBannerSkinnyHeaderBannerItemViewModel(string title, string titleImage, string heroImageDesktop, string heroImageTablet, string heroImageMobile, string subtitle, string bannerStyle, string htmlContent, string breadcrumbs)
            : base(title, titleImage, heroImageDesktop, heroImageTablet, heroImageMobile, subtitle, bannerStyle, htmlContent)
        {
            Breadcrumbs = breadcrumbs;
        }

        public string Breadcrumbs { get; set; }
    }

    public class WideBannerStripeBannerItemViewModel : WideBannerItemViewModel
    {
        public WideBannerStripeBannerItemViewModel(string title, string titleImage, string heroImageDesktop, string heroImageTablet, string heroImageMobile, string subtitle, string bannerStyle, string htmlContent)
            : base(title, titleImage, heroImageDesktop, heroImageTablet, heroImageMobile, subtitle, bannerStyle, htmlContent)
        {
        }
    }

    public class WideBannerViewModel : PageViewModelBase
    {
        public string Title { get; set; }
        public IList<WideBannerItemViewModel> Banners { get; set; }
    }
}
