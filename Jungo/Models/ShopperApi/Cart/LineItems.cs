using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Cart
{
    public class LineItems : ResourceLink
    {
        public LineItem[] LineItem { get; set; }
    }
}
