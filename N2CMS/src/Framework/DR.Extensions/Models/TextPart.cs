using System.Web.UI.WebControls;
using N2.Details;

namespace N2.Models
{
    [PartDefinition(Title = "Text (WYSIWYG)", SortOrder = 100, IconUrl = "{IconsUrl}/text_align_left.png")]
    public class TextPart : PartModelBase
    {
        /// <summary>
        /// The text to render on the page
        /// </summary>
        [EditableFreeTextArea]
        public virtual string Text
        {
            get { return (string)(GetDetail("Text") ?? string.Empty); }
            set { SetDetail("Text", value, string.Empty); }
        }
    }

	[PartDefinition(Title = "Html (Raw)", SortOrder = 101, IconUrl = "{IconsUrl}/tag.png")]
    public class HtmlPart : PartModelBase
    {
        /// <summary>
        /// The raw html to render on the page
        /// </summary>
        [EditableSource(Title="Raw Html (no Razor/ASP)", IsIndexable=false)]
        public virtual string Text {
            get { return (string)(GetDetail("Text") ?? string.Empty); }
            set { SetDetail("Text", value, string.Empty); }
        }

        /// <summary>
        /// Editor's notes, e.g. what this snippet is good for
        /// </summary>
        [EditableTextBox(Title = "Notes", IsIndexable = false, TextMode = TextBoxMode.MultiLine)]
        public virtual string Notes
        {
            get { return (string)(GetDetail("Notes") ?? string.Empty); }
            set { SetDetail("Notes", value, string.Empty); }
        }
    }
}
