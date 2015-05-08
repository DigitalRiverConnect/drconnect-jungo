using System.Collections.Generic;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;
using N2.Web.UI.WebControls;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Product List",
        Description = "Product List",
        SortOrder = 159,
        IconUrl = "~/Content/img/icons/package.png")]
    [WithEditableTitle("Title", 20, Focus = false, Required = false)]
    public class ProductListPart : PartModelBase
    {
        [EditableChildren(Title = "Products", ZoneName = "Products", SortOrder = 90, ContainerName = Defaults.Containers.Content)]
        public IList<ProductItem> Products
        {
            get { return GetChildren<ProductItem>("Products"); }
        }
    }

    [PartDefinition("Product List Item",
        Description = "Product List Item",
        SortOrder = 10,
        IconUrl = "~/Content/img/icons/product16.png")]
    [RestrictParents(typeof (ProductListPart))]
    [AllowedZones("Products")] // ensure not added on pages directly
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class ProductItem : PartModelBase
    {
        [EditableProductSelection("ID:Product; Name:[Title]; Url:TargetUrl", Title = "Product", SortOrder = 50)]
        public string Product
        {
            get { return ((string) GetDetail("Product") ?? ""); }
            set { SetDetail("Product", value, ""); }
        }

        [EditableText("Text override", 103)]
        public string Text
        {
            get { return ((string) GetDetail("Text") ?? ""); }
            set { SetDetail("Text", value, ""); }
        }

        [EditableUrl(title: "URL override", sortOrder: 104, RelativeTo = UrlRelativityMode.Application)]
        public string TargetUrl
        {
            get { return (string) (GetDetail("TargetUrl") ?? string.Empty); }
            set { SetDetail("TargetUrl", value, string.Empty); }
        }
    }
}
