using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Catalog Page",
        Description = "A catalog page.",
        SortOrder = 210,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/page.png")]
    [WithEditableName]
    public class CatalogPage : CachingPageBase
    {
        [EditableCategorySelection("ID:CategoryID;Name:[Title]", Title = "For Category ID", SortOrder = 101)]
        public virtual string CategoryID
        {
            get { return (string)(GetDetail("CategoryID") ?? string.Empty); }
            set { SetDetail("CategoryID", value, string.Empty); }
        }
    }
}