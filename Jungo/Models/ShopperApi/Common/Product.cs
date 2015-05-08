using Jungo.Models.ShopperApi.Catalog;

namespace Jungo.Models.ShopperApi.Common
{
    public class Product : ProductBrief
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string ProductType { get; set; }
        public string Sku { get; set; }
        public string ExternalReferenceId { get; set; }
        public string CompanyId { get; set; }
        public bool DisplayableProduct { get; set; }
        public bool Purchasable { get; set; }
        public string ManufacturerName { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public int? MinimumQuantity { get; set; }
        public int? MaximumQuantity { get; set; }
        public string ProductImage { get; set; }
        public bool BaseProduct { get; set; }
        public ResourceLink ParentProduct { get; set; } // present if baseProduct false and has a parent
        public InventoryStatus InventoryStatus { get; set; }
        public CustomAttributes CustomAttributes { get; set; }
    }
}
