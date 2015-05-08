using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Details;
using N2.Integrity;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Simple Content Page Link",
        Description = "List of Links - Content Page Link ",
        SortOrder = 70,
        IconUrl = "~/Content/img/icons/favorite_16.png")]
    [RestrictParents(typeof(ListofLinksPart))]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class ContentPageListofLinksItem : ListofLinksItem
    {
        [EditableContentSelection("Name:[ContentPage,TargetUrl]; Title:[Title,LinkText]", Title = "Content Page", SortOrder = 50)]
        public string ContentPage
        {
            get { return ((string)GetDetail("ContentPage") ?? ""); }
            set { SetDetail("ContentPage", value, ""); }
        }

        public override string GetUrl(ILinkGenerator linkGenerator)
        {
            return linkGenerator.GenerateLinkForNamedContentItem(TargetUrl);
        }
    }

    [PartDefinition("Simple Product Link",
        Description = "List of Links - Product Link ",
        SortOrder = 100,
        IconUrl = "~/Content/img/icons/favorite_16.png")]
    [RestrictParents(typeof(ListofLinksPart))]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class ProductListofLinksItem : ListofLinksItem, IProductPart
    {
        [EditableProductSelection("ID:Product; Name:[Title,LinkText]; Url:TargetUrl", Title = "Product", SortOrder = 50)]
        public string Product
        {
            get { return ((string)GetDetail("Product") ?? ""); }
            set { SetDetail("Product", value, ""); }
        }

        public override string GetUrl(ILinkGenerator linkGenerator)
        {
            long pid;
            return linkGenerator.GenerateProductLink(long.TryParse(Product, out pid) ? pid : (long?)null);
        }
    }

    [PartDefinition("Simple Category Link",
        Description = "List of Links - Category Link ",
        SortOrder = 200,
        IconUrl = "~/Content/img/icons/favorite_16.png")]
    [RestrictParents(typeof(ListofLinksPart))]
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class CategoryListofLinksItem : ListofLinksItem
    {
        [EditableCategorySelection("ID:Category; Name:[Title,LinkText]; Url:TargetUrl", Title = "Category", SortOrder = 50)]
        public string Category
        {
            get { return ((string)GetDetail("Category") ?? ""); }
            set { SetDetail("Category", value, ""); }
        }

        [EditableCheckBox("Force List Page", 51, Title = "Force List Page")]
        public bool ForceListPage
        {
            get { return ((bool?)GetDetail("ForceListPage") ?? false); }
            set { SetDetail("ForceListPage", value, false); }
        }

        public override string GetUrl(ILinkGenerator linkGenerator)
        {
            long catId;
            if (!long.TryParse(Category, out catId))
                catId = 0;
            return linkGenerator.GenerateCategoryLink(catId);
        }
    }



}