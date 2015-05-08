using System;

namespace Jungo.Implementations.ShopperApiV1.Config
{
    [Serializable]
    public class ShopperApiUriTemplatesConfig
    {
        // query params:
        public string LimitedPublicTokenQuery { get; set; }
        public string ApiKeyQuery { get; set; }
        public string RefreshTokenQuery { get; set; }
        public string ExpandQuery { get; set; }
        public string ProductsKeywordQuery { get; set; }
        public string ApplyCouponQuery { get; set; }
        public string ChangeQuantityQuery { get; set; }
        public string MultipleProductQuery { get; set; }
        public string AddProductToCartQuery { get; set; }
        public string PagingOptionsQuery { get; set; }
 
        // query param values:
        public string ExpandAllQueryValue { get; set; }
        public string ExpandProductIdQueryValue { get; set; }

        // path segments
        public string IdSegment { get; set; }
        public string ProductOffersSegment { get; set; }
        public string PopOffersSegment { get; set; }
        public string InventoryStatusSegment { get; set; }
    }

    [Serializable]
    public class ConstructUriInfo // todo: delete this class when billboard obtained from shopper api has all we need
    {
        public string SubstituteBillboardFragment { get; set; }
        public string With { get; set; }
        public string Construct(string billboardUri)
        {
            return billboardUri.Replace(SubstituteBillboardFragment, With);
        }
    }
}
