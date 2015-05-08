using Jungo.Models.ShopperApi.Catalog;

namespace Jungo.Infrastructure.Extensions
{
    public static class ProductExtensions
    {
        public static bool HasVariations(this Product product)
        {
            return product.BaseProduct && product.Variations != null && product.Variations.Product != null &&
                    product.Variations.Product.Length > 1;
        }
    }
}
