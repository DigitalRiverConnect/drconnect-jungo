namespace Jungo.Models.ShopperApi.Common
{
    public class Attribute
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class CustomAttributes
    {
        public Attribute[] Attribute { get; set; }
    }
}
