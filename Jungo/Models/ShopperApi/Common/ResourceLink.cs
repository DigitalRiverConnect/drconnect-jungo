namespace Jungo.Models.ShopperApi.Common
{
    public class ResourceUri
    {
        public string Uri { get; set; }
    }

    public class ResourceLink : ResourceUri
    {
        public string Relation { get; set; }
    }
}
