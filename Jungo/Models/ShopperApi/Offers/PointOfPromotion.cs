using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Offers
{
    public class PointOfPromotion : ResourceLink
    {
        public string Id { get; set; }
        public string[] OfferTypes { get; set; }
        public ResourceLink Offers { get; set; }
    }
}
