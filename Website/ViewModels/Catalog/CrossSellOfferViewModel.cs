using Jungo.Models.ShopperApi.Offers;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class CrossSellOfferViewModel : Offer
    {
        public ProductOfferViewModel[] ProductOffersOfferViewModels { get; set; }
    }
}
