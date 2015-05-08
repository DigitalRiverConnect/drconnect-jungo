using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("About Page",
        Description = "About Page",
        SortOrder = 300,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/radial16.png")]
    [WithEditableName]
    public class AboutPage : ContentPage
    {
    }
}