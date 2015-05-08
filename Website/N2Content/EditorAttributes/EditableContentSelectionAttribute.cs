using System.Globalization;
using System.Web.UI;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2.Resources;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public class EditableContentSelectionAttribute : EditableApiSelectorAttribute
    {
        public EditableContentSelectionAttribute(string propertyMap)
            :base(propertyMap)
        {}

        protected override void LoadControlScripts(Control container)
        {
            container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/editors/contentPicker.js"));
        }
        
        protected override string ButtonClassPrefix
        {
            get { return "content"; }
        }

        protected override string ButtonText
        {
            get { return "Select Content"; }
        }

        protected override void SetCustomAttributes(Control container, System.Web.UI.HtmlControls.HtmlControl controlToDecorate)
        {
            controlToDecorate.Attributes["data-start-page-id"] = GetStartPageId(container).ToString(CultureInfo.InvariantCulture);
        }

        private int GetStartPageId(Control container)
        {
            var itemEditor = GetItemEditor(container);
            if (itemEditor != null)
            {
                var currentItem = itemEditor.CurrentItem;

                while (currentItem != null)
                {
                    var startPage = currentItem as StartPage;

                    if (startPage != null)
                    {
                        return startPage.ID;
                    }

                    currentItem = currentItem.Parent;
                }
            }

            return -1;
        }
    }
}
