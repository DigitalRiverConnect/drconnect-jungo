namespace Jungo.Models.ShopperApi.Common
{
    public class ResourceLinkPaged : ResourceLink
    {
        public int TotalResults { get; set; }
        public int TotalResultPages { get; set; }
    }
}
