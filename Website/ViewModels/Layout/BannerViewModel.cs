using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class BannerViewModel
    {
        public BannerViewModel()
        {
            Slides = new List<BannerSlideViewModel>();
        }

        public IList<BannerSlideViewModel> Slides { get; private set; }
    }

    public class BannerSlideViewModel
    {
        public string BannerCaption { get; set; }
        public string BannerImageUrl { get; set; }
        public string BannerLinkUrl { get; set; }
        public string CallToAction { get; set; }
        public string ButtonText { get; set; }
    }
}
