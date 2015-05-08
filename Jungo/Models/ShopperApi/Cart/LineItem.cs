using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Cart
{
    public class LineItem : ResourceLink
    {
        public long Id { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public Pricing Pricing { get; set; }
    }
}
