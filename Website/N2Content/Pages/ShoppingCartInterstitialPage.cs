using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Details;
using N2.Installation;
using N2.Integrity;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Shopping Cart Interstitial Page",
        Description = "A product detail page.",
        SortOrder = 235,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/advert16.png")]
    [WithEditableName]
    [RestrictParents(typeof(StoreHomePage), typeof(ProductPage), typeof(CatalogPage))]
    public class ShoppingCartInterstitialPage : PageModelBase
    {
        [EditableProductMultiSelection("Id:ProductID; Title:Title;", Title = "Product List", SortOrder = 101)]
        public virtual string ProductID { get; set; }

        // TODO: Can't do Categories for now, due to the fact that GC/Cloudlink only has *soft* categories. That is,
        // TODO: Categories contain products, but we cannot retrieve the category that a producct belongs to.
        //[EditableCategorySelection("Id:CategoryID; Title:Title;", Title = "Category", SortOrder = 200)]
        //public virtual string CategoryID { get; set; }

    }
}
