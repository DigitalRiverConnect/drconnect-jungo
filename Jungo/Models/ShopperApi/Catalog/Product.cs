using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class Product : Common.Product
    {
        public Pricing Pricing { get; set; }
        public AddProductToCartLink AddProductToCart { get; set; } // present if baseProduct false
        public Variations Variations { get; set; } // present if baseProduct true
        public VariationAttributes VariationAttributes { get; set; } // possibly present if baseProduct true
    }
}
