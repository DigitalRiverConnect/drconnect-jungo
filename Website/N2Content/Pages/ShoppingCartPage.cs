using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Shopping Cart Page",
        Description = "A shopping cart page.",
        SortOrder = 230,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/cart16.png")]
    [WithEditableName]
    public class ShoppingCartPage : PageModelBase
    {
    }
}