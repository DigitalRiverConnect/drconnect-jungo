using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class Variations : ResourceLink
    {
        public Product[] Product { get; set; } // for each Product, BaseProduct will always be false, so Variations will always be null
    }
}
