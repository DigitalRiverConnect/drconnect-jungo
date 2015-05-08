using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using N2;
using N2.Details;
using N2.Resources;
using N2.Web.UI.WebControls;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public abstract class EditableApiSelectorAttribute: AbstractEditableAttribute
    {
        protected EditableApiSelectorAttribute()
        {
        }

        protected EditableApiSelectorAttribute(string propertyMap)
        {
            Initialize(propertyMap);
        }

        protected EditableApiSelectorAttribute(string propertyMap, string title, int sortOrder)
            : base(title, sortOrder)
        {
            Initialize(propertyMap);
        }

        protected EditableApiSelectorAttribute(string propertyMap, string title, string name, int sortOrder)
            : base(title, name, sortOrder)
        {
            Initialize(propertyMap);
        }

        public override bool UpdateItem(ContentItem item, Control editor)
        {
            var textBox = (TextBox)editor.FindControl(Name);
            item[Name] = textBox.Text;

            return true;
        }

        public override void UpdateEditor(ContentItem item, Control editor)
        {
            var textBox = (TextBox)editor.FindControl(Name);
            textBox.Text = (string)item[Name];
        }

        public string PropertyMap { get; set; }

        protected override Control AddEditor(Control container)
        {
            // Register the javascript needed for the control.
            container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/underscore-min.js"));
            container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/backbone-min.js"));
            LoadControlScripts(container);
            var control = new Control();
            var textBox = new TextBox {CssClass = "textEditor", ID = Name};
            textBox.ID = textBox.ID;
            control.Controls.Add(textBox);

            var csButton = new HtmlInputButton();

            csButton.Attributes["value"] = ButtonText;
            csButton.Attributes["class"] = string.Format("{0}-selector-button", ButtonClassPrefix);
            csButton.Attributes["data-app-path"] = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath;
            csButton.Attributes["data-image-control-id"] = textBox.ClientID;
            if (PropertyMap != null)
                csButton.Attributes["data-property-map"] = PropertyMapToJson();
            SetCustomAttributes(container, csButton);
            control.Controls.Add(csButton);

            container.Controls.Add(control);
            return control;
        }

        protected string PropertyMapToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");

            foreach (var mapItem in PropertyMap.Split(';'))
            {
                var nameValue = mapItem.Split(':');
                if (nameValue.Length == 2)
                {
                    if (nameValue[1].StartsWith("["))
                    {
                        nameValue[1] = string.Format("[{0}]",
                            string.Join(",",
                                nameValue[1]
                                    .TrimStart('[')
                                    .TrimEnd(']')
                                    .Trim()
                                    .Split(',').Select(v => string.Format("\"{0}\"", v))));
                    }
                    else
                    {
                        nameValue[1] = string.Format("\"{0}\"", nameValue[1].Trim());
                    }

                    sb.Append("\"");
                    sb.Append(nameValue[0].Trim());
                    sb.Append("\":");
                    sb.Append(nameValue[1]);
                    sb.Append(",");
                }
            }

            if (sb.Length > 1)
            {
                sb.Length -= 1;
                sb.Append("}");
            }


            return sb.ToString();
        }

        protected virtual void LoadControlScripts(Control container)
        {
            
        }

        protected virtual void SetCustomAttributes(Control container, HtmlControl controlToDecorate)
        {
        }

        protected abstract string ButtonText { get; }

        protected abstract string ButtonClassPrefix { get; }

        protected ItemEditor GetItemEditor(Control container)
        {
            var currentItem = container;
            while (currentItem != null)
            {
                var itemEditor = currentItem as ItemEditor;
                if (itemEditor != null)
                    return itemEditor;

                currentItem = currentItem.Parent;
            }

            return null;
        }

        private void Initialize(string propertyMap)
        {
            PropertyMap = propertyMap ?? string.Empty;
        }
    }
}
