using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Shopping Cart Warning Page",
        Description = "A page to display Shopping Cart Warning.",
        SortOrder = 385,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/page.png")]
    [WithEditableName]
    public class ShoppingCartWarningPage : ContentPage
    {
        public override bool IncludeInSitemap()
        {
            return false;
        }
    }
}