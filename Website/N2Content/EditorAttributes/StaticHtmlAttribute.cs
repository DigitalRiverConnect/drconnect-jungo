using System;
using N2;
using N2.Details;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    // TODO - do we really want this?
    [AttributeUsage(AttributeTargets.Property)]
    public class StaticHtmlAttribute : AbstractEditableAttribute
    {
        public StaticHtmlAttribute(int sortOrder) : base("", sortOrder)
        {
        }

        public override bool UpdateItem(ContentItem item, System.Web.UI.Control editor)
        {
            return false;
        }

        public override void UpdateEditor(ContentItem item, System.Web.UI.Control editor)
        {
            var div = (System.Web.UI.HtmlControls.HtmlGenericControl)editor;
            var value = item[Name] as string;
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            div.InnerHtml = value;
        }

        protected override System.Web.UI.Control AddEditor(System.Web.UI.Control container)
        {
            var div = new System.Web.UI.HtmlControls.HtmlGenericControl("div") {ID = Name, InnerHtml = string.Empty};
            container.Controls.Add(div);
            return div;
        }
    }
}
