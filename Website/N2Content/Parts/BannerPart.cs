using System.Collections.Generic;
using System.Web.UI.WebControls;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Hero Banner (rolling)",
        Description = "Hero Banner (rolling)",
        SortOrder = 157,
        IconUrl = "~/Content/img/icons/package.png")]
    public class BannerPart : PartModelBase
    {
        [EditableChildren(Title = "Slides", ZoneName = "Slides", SortOrder = 210, ContainerName = Defaults.Containers.Content)]
        public IList<BannerItem> Slides
        {
            get { return GetChildren<BannerItem>("Slides"); }
        }
    }

    [PartDefinition("Banner Slide",
        Description = "Banner Slide",
        SortOrder = 10,
        IconUrl = "~/Content/img/icons/advert16.png")]
    [RestrictParents(typeof(BannerPart))]
    [AllowedZones("Slides")] // ensure not added on pages directly
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class BannerItem : PartModelBase
    {
        [EditableImageSelection("", Title = "Banner Image URL", SortOrder = 107)]
        public string BannerImageUrl
        {
            get { return (string)(GetDetail("BannerImageUrl") ?? string.Empty); }
            set { SetDetail("BannerImageUrl", value, string.Empty); }
        }

        [EditableUrl("Banner Link URL", 109)]
        public string BannerLinkUrl
        {
            get { return (string)(GetDetail("BannerLinkUrl") ?? string.Empty); }
            set { SetDetail("BannerLinkUrl", value, string.Empty); }
        }

        [EditableText(title: "Call to action text", sortOrder: 150, TextMode = TextBoxMode.MultiLine)]
        public string CallToAction
        {
            get { return (string)(GetDetail("CallToAction") ?? string.Empty); }
            set { SetDetail("CallToAction", value, string.Empty); }
        }

        [EditableText(title: "Button text", sortOrder: 160, TextMode = TextBoxMode.MultiLine)]
        public string ButtonText
        {
            get { return (string)(GetDetail("ButtonText") ?? string.Empty); }
            set { SetDetail("ButtonText", value, string.Empty); }
        }
    }

    [PartDefinition("Product Banner Slide",
       Description = "Product Banner Slide",
       SortOrder = 157,
       IconUrl = "~/Content/img/icons/advert16.png")]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class ProductBannerItem : BannerItem, IProductPart
    {
        [EditableProductSelection("ID:Product; Name:Title; Url:BannerLinkUrl; ImageUrl:BannerImageUrl", Title = "Product", SortOrder = 50)]
        public string Product
        {
            get { return ((string)GetDetail("Product") ?? ""); }
            set { SetDetail("Product", value, ""); }
        }
    }


    [PartDefinition("Category Banner Slide",
       Description = "Category Banner Slide",
       SortOrder = 157,
       IconUrl = "~/Content/img/icons/advert16.png")]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class CategoryBannerItem : BannerItem
    {
        [EditableCategorySelection("ID:Category; Name:Title; Url:BannerLinkUrl; ImageUrl:BannerImageUrl", Title = "Category", SortOrder = 50)]
        public string Category
        {
            get { return ((string)GetDetail("Category") ?? ""); }
            set { SetDetail("Category", value, ""); }
        }
    }
}
