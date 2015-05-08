using System.Web.UI;
using N2.Resources;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public class EditableProductSelectionAttribute : EditableCatalogItemSelectorAttribute
    {
        public EditableProductSelectionAttribute(string propertyMap)
            : base(propertyMap)
        {
        }

        protected override void LoadControlScripts(Control container)
        {
            container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/editors/productPicker.js"));
        }

        protected override string ButtonClassPrefix
        {
            get { return "product"; }
        }


        protected override string ButtonText
        {
            get { return "Select Product"; }
        }
    }
}
