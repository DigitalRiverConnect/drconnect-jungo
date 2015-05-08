using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Offers
{
    public class ProductOffer : ResourceLink
    {
        public long Id { get; set; }
        public Product Product { get; set; }
        public AddProductToCartLink AddProductToCart { get; set; }
        public string[] SalesPitch { get; set; }
        public string Image { get; set; }
        public PricingWithDiscount Pricing { get; set; }
    }
}
