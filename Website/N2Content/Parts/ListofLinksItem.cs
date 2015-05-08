using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;
using N2.Web.UI.WebControls;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("Link",
        Description = "List of Links - Link ",
        SortOrder = 10,
        IconUrl = "~/Content/img/icons/favorite_16.png")]
    [RestrictParents(typeof(ListofLinksPart))]
    [AllowedZones("Offers", "Links")] // ensure not added on pages directly
    [WithEditableTitle("Title", 90, Focus = false, Required = false)]
    public class ListofLinksItem : PartModelBase
    {
        // TODO this property has a mismatch between identifier and detail name, leading to export/import issues
        [EditableText("Link Text", 103)]
        public string LinkText
        {
            get
            {
                return ((string)GetDetail("LinkText") ?? (string)GetDetail("LinkCopy") ?? "");
            }
            set { 
                SetDetail("LinkCopy", value, ""); // deprecated name, left for backwards compatibility
                SetDetail("LinkText", value, ""); // correct name
            }
        }

        [EditableUrl(title: "Target URL", sortOrder: 104, RelativeTo = UrlRelativityMode.Application)]
        public string TargetUrl
        {
            get { return (string)(GetDetail("TargetUrl") ?? string.Empty); }
            set { SetDetail("TargetUrl", value, string.Empty); }
        }

        [EditableText(title: "URL Suffix", sortOrder: 110, HelpText = "This text will be appended to the end of the generated URL.")]
        public string UrlSuffix
        {
            get { return (string)(GetDetail("UrlSuffix") ?? string.Empty); }
            set { SetDetail("UrlSuffix", value, string.Empty); }
        }

        [EditableText(title: "Target", sortOrder: 120, HelpText = "This text will be target attribute of the link.")]
        public string Target
        {
            get { return (string)(GetDetail("Target") ?? string.Empty); }
            set { SetDetail("Target", value, string.Empty); }
        }

        [EditableCheckBox(Title = "Suppress Links", Name = "SuppressLinks", SortOrder = 1000, CheckBoxText = "")]
        public bool SuppressLinks
        {
            get { return ((bool?)GetDetail("SuppressLinks") ?? false); }
            set { SetDetail("SuppressLinks", value, false); }
        }

        public virtual string GetUrl(ILinkGenerator linkGenerator)
        {
            return TargetUrl;
        }

        public override string ToString()
        {
            return base.ToString() + " " + LinkText;
        }
    }

}