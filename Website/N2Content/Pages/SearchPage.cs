using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Search Page",
        Description = "A search page.",
        SortOrder = 211,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/page.png")]
    [WithEditableName]
    public class SearchPage : CachingPageBase
    {
    }
}
