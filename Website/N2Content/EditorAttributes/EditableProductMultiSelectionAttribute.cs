using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public class EditableProductMultiSelectionAttribute : EditableProductSelectionAttribute
    {
        public EditableProductMultiSelectionAttribute(string propertyMap)
            : base(propertyMap)
        {
        }

        protected override void SetCustomAttributes(Control container, HtmlControl controlToDecorate)
        {
            controlToDecorate.Attributes["data-is-multi-select"] = "true";
            base.SetCustomAttributes(container, controlToDecorate);

            //var parentCategoryId = GetParentCategoryId(container);
            //if (!string.IsNullOrEmpty(parentCategoryId))
            //    controlToDecorate.Attributes["data-parent-category-id"] = parentCategoryId;
        }


        //protected override void LoadControlScripts(Control container)
        //{
        //    container.Page.JavaScript(Engine.ManagementPaths.ResolveResourceUrl("~/Scripts/editors/productPicker.js"));
        //}

        //protected override string ButtonClassPrefix
        //{
        //    get { return "product"; }
        //}

        //protected override string ButtonText
        //{
        //    get { return "Select Product"; }
        //}
    }
}
