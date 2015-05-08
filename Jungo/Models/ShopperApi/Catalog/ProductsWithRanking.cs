using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class ProductsWithRanking : ResourceLinkPaged
    {
        public ProductWithRanking[] Product { get; set; }
    }
}
