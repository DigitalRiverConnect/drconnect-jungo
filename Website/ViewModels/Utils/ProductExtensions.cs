using System;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Utils
{
    public static class ProductExtensions
    {
        public static Product GetDisplayProduct(this Product product, bool skipStockAvailabilityCheck = true)
        {
            return product.Variations == null || product.Variations.Product == null ||
                   product.Variations.Product.Length == 0
                ? product
                : product.Variations.Product[0];
        }


        public static string GetParentTitle(this Product product)
        {
            return product.DisplayName;
        }

        public static long GetParentProductId(this Product product)
        {
            if (product.BaseProduct)
                return product.Id;
            if (product.ParentProduct != null)
            {
                // todo: 1st: why is this method needed in the first place? 2nd: we shouldn't have to parse the id out of the URI
                var idx = product.ParentProduct.Uri.LastIndexOf("/", StringComparison.Ordinal);
                if (idx <= 0) return 0;
                var idstr = product.ParentProduct.Uri.Substring(idx + 1);
                long id;
                return long.TryParse(idstr, out id) ? id : 0;
            }
            return 0;
        }
    }
}
