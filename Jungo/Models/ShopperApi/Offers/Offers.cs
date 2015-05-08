using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Offers
{
    public class Offers : ResourceLinkPaged
    {
        public Offer[] Offer { get; set; }
    }
}
