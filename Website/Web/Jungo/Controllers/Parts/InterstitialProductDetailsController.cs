using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(InterstitialProductDetailsPart))]
    public class InterstitialProductDetailsController : PartControllerBase<InterstitialProductDetailsPart>
    {
        public InterstitialProductDetailsController(IRequestLogger logger)
            : base(logger)
        {
        }

        public override ActionResult Index()
        {
            var model = WebSession.Current.Get<ProductDetailPageViewModel>(WebSession.CurrentProductSlot);
            return PartialView(model);
        }
    }
}