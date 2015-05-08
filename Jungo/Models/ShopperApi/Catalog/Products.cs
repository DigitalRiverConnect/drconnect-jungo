using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class Products : ResourceLinkPaged
    {
        public Product[] Product { get; set; }
    }
}
