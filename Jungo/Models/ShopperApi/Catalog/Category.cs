using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class Category : ResourceLink
    {
        public long Id { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string ThumbnailImage { get; set; }
        public CustomAttributes CustomAttributes { get; set; }
        public Products Products { get; set; }
        public Categories Categories { get; set; }
    }
}
