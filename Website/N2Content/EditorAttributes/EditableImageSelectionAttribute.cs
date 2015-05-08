using System;
using System.Web.UI;
using N2.Resources;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableImageSelectionAttribute : EditableApiSelectorAttribute
    {
        public EditableImageSelectionAttribute(string propertyMap)
            : base(propertyMap)
        {
        }

        protected override void LoadControlScripts(Control container)
        {
            container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/editors/imagePicker.js"));
        }

        protected override string ButtonClassPrefix
        {
            get { return "image"; }
        }

        protected override string ButtonText
        {
            get { return "Select Image"; }
        }
    }
}
