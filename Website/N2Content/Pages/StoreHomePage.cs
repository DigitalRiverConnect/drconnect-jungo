using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Store Home Page",
        Description = "Store Home Page",
        SortOrder = 280,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/sale16.png")]
    [WithEditableName]
    public class StoreHomePage : ContentPage
    {
    }
}