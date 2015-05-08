using System.Web.UI.WebControls;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition(Title = "JavaScript", SortOrder = 101, IconUrl = "{IconsUrl}/tag.png")]
    [AllowedZones("Javascript", "RecursiveJavascript", "JavascriptFooter", "RecursiveJavascriptFooter")] // ensure not added on pages directly 
    public class JavaScriptPart : PartModelBase
    {
        /// <summary>
        /// A Script Element to render on the page
        /// </summary>
        [EditableSource(Title = "Script Element Text (do not include script tag itself, only the javascript content)", IsIndexable = false)]
        public virtual string Text
        {
            get { return (string)(GetDetail("Text") ?? string.Empty); }
            set { SetDetail("Text", value, string.Empty); }
        }

        /// <summary>
        /// Editor's notes, e.g. what this Script Element is good for
        /// </summary>
        [EditableTextBox(Title = "Notes", IsIndexable = false, TextMode = TextBoxMode.MultiLine)]
        public virtual string Notes
        {
            get { return (string)(GetDetail("Notes") ?? string.Empty); }
            set { SetDetail("Notes", value, string.Empty); }
        }
    }
}
