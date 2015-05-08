using System;
using System.Web.UI;
using N2.Resources;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableCategorySelectionAttribute : EditableCatalogItemSelectorAttribute
    {
        public EditableCategorySelectionAttribute(string propertyMap)
            : base(propertyMap)
        {
        }

        protected override void LoadControlScripts(Control container)
        {
            container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/editors/categoryPicker.js"));
        }

        protected override string ButtonClassPrefix
        {
            get { return "category"; }
        }

        protected override string ButtonText
        {
            get { return "Select Category"; }
        }
    }
}
