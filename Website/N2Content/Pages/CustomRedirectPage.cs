using System.Linq;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Custom Redirect Page",
        Description = "Redirect this URL to a different page.",
        SortOrder = 320,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/redirect16.png")]
    [WithEditableName]
    public class CustomRedirectPage : CachingPageBase
    {
        [StaticHtml(210)]
        public string Explanation
        {
            get
            {
                return
                    "Either redirect to a content page on this site, or an external URL.";
            }
        }

        [EditableContentSelection("Name:[ContentPageName]; Title:[ContentPageTitle]", Title = "Content Page", SortOrder = 220)]
        public string ContentPageName
        {
            get { return ((string)GetDetail("ContentPageName") ?? ""); }
            set { SetDetail("ContentPageName", value, ""); }
        }

        [EditableText(ReadOnly = true, SortOrder = 225, Title="Content Page Title")]
        public string ContentPageTitle
        {
            get { return (string)(GetDetail("ContentPageTitle") ?? GetContentPageTitle()); }
            set { SetDetail("ContentPageTitle", value, string.Empty); }
        }


        [EditableText(title: "External URL", sortOrder: 230, HelpText = "A link outside this site.")]
        public string ExternalUrl
        {
            get { return (string)(GetDetail("ExternalUrl") ?? string.Empty); }
            set { SetDetail("ExternalUrl", value, string.Empty); }
        }

        public string GetUrl(ILinkGenerator linkGenerator)
        {
            return !string.IsNullOrEmpty(ExternalUrl)
                ? ExternalUrl
                : linkGenerator.GenerateLinkForNamedContentItem(ContentPageName);
        }

        private string GetContentPageTitle()
        {
            var contentPageName = ContentPageName;
            var contentPage = CmsFinder.FindAll<ContentItem>().FirstOrDefault(c => c.Name == contentPageName);
            return contentPage == null ? string.Empty : contentPage.Title;
        }
    }
}
