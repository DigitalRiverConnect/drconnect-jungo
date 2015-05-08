using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(OfferListPart))]
    public class OfferListController : ContentControllerBase<OfferListPart>
    {
        private readonly IOfferListViewModelBuilder _offerListViewModelBuilder;

        public OfferListController(IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi, IOfferListViewModelBuilder offerListViewModelBuilder)
            : base(logger, linkGenerator, catalogApi)
        {
            _offerListViewModelBuilder = offerListViewModelBuilder;
        }

        public override ActionResult Index()
        {
            return PartialView(_offerListViewModelBuilder.GetOfferListAsync(CurrentItem).Result);
        }
    }
}