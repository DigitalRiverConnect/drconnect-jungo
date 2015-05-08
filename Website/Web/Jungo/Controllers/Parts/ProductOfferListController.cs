using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(ProductOfferListPart))]
    public class ProductOfferListController : ContentControllerBase<ProductOfferListPart>
    {
        private readonly IOfferListViewModelBuilder _offerListViewModelBuilder;

        public ProductOfferListController(IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi, IOfferListViewModelBuilder offerListViewModelBuilder)
            : base(logger, linkGenerator, catalogApi)
        {
            _offerListViewModelBuilder = offerListViewModelBuilder;
        }

        public override ActionResult Index()
        {
            var ppart = CurrentPage as IProductPart;
            return PartialView(_offerListViewModelBuilder.GetProductOfferListAsync(CurrentItem, ppart, CatalogApi).Result);
        }
    }
}