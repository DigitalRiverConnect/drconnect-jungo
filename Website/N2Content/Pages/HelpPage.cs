using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Help Page",
        Description = "Help Page",
        SortOrder = 290,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/help.png")]
    [WithEditableName]
    public class HelpPage : ContentPage
    {
    }
}