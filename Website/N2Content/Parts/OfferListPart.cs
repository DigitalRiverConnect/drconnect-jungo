using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Offer List",
       Description = "Offer List",
       SortOrder = 160,
       IconUrl = "~/Content/img/icons/advert16.png")]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class OfferListPart : PartModelBase
    {
        [EditablePopSelection(title: "Point-of-Promotion Name", sortOrder: 100)]
        public string PopName
        {
            get { return (string)(GetDetail("PopName") ?? string.Empty); }
            set { SetDetail("PopName", value, string.Empty); }
        }


        [EditableNumber(Title = "Maximum Number of Products Shown", SortOrder = 110)]
        public int MaxNProducts
        {
            get { return ((int?)GetDetail("MaxNProducts") ?? 0); }
            set { SetDetail("MaxNProducts", value, null); }
        }
    }
}
