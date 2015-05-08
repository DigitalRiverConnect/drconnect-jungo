using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Offers
{
    public class Offer : ResourceLink
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string[] SalesPitch { get; set; }
        public CustomAttributes CustomAttributes { get; set; }
        public ProductOffers ProductOffers { get; set; }
        public OfferBundleGroups OfferBundleGroups { get; set; }
    }
}
