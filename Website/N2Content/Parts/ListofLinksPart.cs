using System.Collections.Generic;
using System.Web.UI.WebControls;
using N2;
using N2.Details;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("List of Links",
        Description = "This provides a list of links",
        SortOrder = 159,
        IconUrl = "~/Content/img/icons/package.png")]
    [WithEditableTitle("Title", 90, Focus = false, Required=false)]
    public class ListofLinksPart : PartModelBase
    {
        [EditableText(title: "Subtitle", sortOrder: 102, TextMode = TextBoxMode.MultiLine)]
        public string Subtitle
        {
            get { return (string)(GetDetail("Subtitle") ?? string.Empty); }
            set { SetDetail("Subtitle", value, string.Empty); }
        }
        
        [EditableText("Background Color", 103)]
        public string BackgroundColor
        {
            get { return (string)(GetDetail("BackgroundColor") ?? "White"); }
            set { SetDetail("BackgroundColor", value, string.Empty); }
        }

        [EditableText("Foreground Color", 104)]
        public string ForegroundColor
        {
            get { return (string)(GetDetail("ForegroundColor") ?? "White"); }
            set { SetDetail("ForegroundColor", value, string.Empty); }
        }

        [EditableCheckBox("Use Button Links", 105)]
        public bool UseButton
        {
            get { return (bool)(GetDetail("UseButton") ?? false); }
            set { SetDetail("UseButton", value); }
        }

        [EditableNumber(Title = "Template Items", SortOrder = 180)]
        public int TemplateItems
        {
            get { return ((int?)GetDetail("TemplateItems") ?? 0); }
            set { SetDetail("TemplateItems", value, null); }
        }

        [EditableChildren(Title = "Links", ZoneName = "Links", SortOrder = 210, ContainerName = Defaults.Containers.Content)]
        public IList<ListofLinksItem> Links
        {            
            get { return GetChildren<ListofLinksItem>("Links");  }
        }

    }
}