using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class ProductBrief : ResourceLink
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string ThumbnailImage { get; set; }
    }
}
