using N2;
using N2.Definitions;
using N2.Details;
using N2.Installation;
using N2.Web.UI;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    /// <summary>
    /// Content page with editable text, not a general base class!
    /// </summary>
    [PageDefinition("Content Page", InstallerVisibility = InstallerHint.NeverRootOrStartPage)]
    [TabContainer(Defaults.Containers.Navigation, "Links", 1050)]
    public class ContentPage : CachingPageBase, IContentPage, IStructuralPage
    {
        /// <summary>
        /// Main content of this content item.
        /// </summary>
        [EditableFreeTextArea(SortOrder = 201, ContainerName = Defaults.Containers.Content)]
        public virtual string Text { get; set; }
    }
}
