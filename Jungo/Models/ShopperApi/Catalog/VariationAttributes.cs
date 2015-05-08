namespace Jungo.Models.ShopperApi.Catalog
{
    public class VariationAttribute
    {
        public string Name { get; set; }
        public string[] DomainValues { get; set; }
    }

    public class VariationAttributes
    {
        public VariationAttribute[] Attribute { get; set; }
    }
}
