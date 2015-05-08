using System.Collections.Generic;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;
using N2.Web.UI.WebControls;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Category List",
        Description = "Category List",
        SortOrder = 159,
        IconUrl = "~/Content/img/icons/package.png")]
    [WithEditableTitle("Title", 19, Focus = false, Required = false)]
    public class CategoryListPart : PartModelBase
    {
        [EditableChildren(Title = "Categories", ZoneName = "Categories", SortOrder = 90, ContainerName = Defaults.Containers.Content)]
        public IList<CategoryItem> Categories
        {
            get { return GetChildren<CategoryItem>("Categories"); }
        }
    }

    [PartDefinition("Category List Item",
        Description = "Category List Item",
        SortOrder = 10,
        IconUrl = "~/Content/img/icons/product16.png")]
    [RestrictParents(typeof(CategoryListPart))]
    [AllowedZones("Categories")] // ensure not added on pages directly
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class CategoryItem : PartModelBase
    {
        [EditableCategorySelection("ID:Category; Name:[Title]; Url:TargetUrl", Title = "Category", SortOrder = 50)]
        public string Category
        {
            get { return ((string)GetDetail("Category") ?? ""); }
            set { SetDetail("Category", value, ""); }
        }

        [EditableText("Text override", 103)]
        public string Text
        {
            get { return ((string)GetDetail("Text") ?? ""); }
            set { SetDetail("Text", value, ""); }
        }

        [EditableUrl(title: "URL override", sortOrder: 104, RelativeTo = UrlRelativityMode.Application)]
        public string TargetUrl
        {
            get { return (string)(GetDetail("TargetUrl") ?? string.Empty); }
            set { SetDetail("TargetUrl", value, string.Empty); }
        }
    }
}
