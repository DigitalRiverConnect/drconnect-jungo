using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(BannerPart))]
    public class BannerController : ContentControllerBase<BannerPart>
    {
        public BannerController(IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
        }

        public override ActionResult Index()
        {
            return PartialView(GetBannerViewModel(CurrentItem));
        }

        private BannerViewModel GetBannerViewModel(BannerPart bannerPart)
        {
            var model = new BannerViewModel();
            foreach (var slide in bannerPart.Slides)
            {
                var slideVm = new BannerSlideViewModel
                {
                    BannerCaption = slide.Title,
                    BannerImageUrl = slide.BannerImageUrl,
                    BannerLinkUrl = slide.BannerLinkUrl,
                    CallToAction = slide.CallToAction,
                    ButtonText = slide.ButtonText
                };
                var bp = slide as ProductBannerItem;
                long id;
                if (bp != null)
                {
                    slideVm.BannerLinkUrl = this.AssureHttpUrl(LinkGenerator.GenerateProductLink(long.TryParse(bp.Product, out id) ? id : (long?)null));
                }
                else
                {
                    var cp = slide as CategoryBannerItem;
                    if (cp != null)
                    {
                        slideVm.BannerLinkUrl = this.AssureHttpUrl(LinkGenerator.GenerateCategoryLink(long.TryParse(cp.Category, out id) ? id : (long?)null));
                    }
                }
                model.Slides.Add(slideVm);
            }
            return model;
        }
    }
}
