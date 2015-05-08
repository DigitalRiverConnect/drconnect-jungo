using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Product Offer List",
       Description = "Product Offer List",
       SortOrder = 161,
       IconUrl = "~/Content/img/icons/advert16.png")]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class ProductOfferListPart : OfferListPart
    {
        [EditableProductSelection("ID:Product", Title = "Product", SortOrder = 105)]
        public string Product
        {
            get { return ((string)GetDetail("Product") ?? ""); }
            set { SetDetail("Product", value, ""); }
        }
    }
}
