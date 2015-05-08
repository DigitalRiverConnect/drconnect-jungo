using System.Web.UI;
using System.Web.UI.HtmlControls;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public abstract class EditableCatalogItemSelectorAttribute : EditableApiSelectorAttribute
    {
        protected EditableCatalogItemSelectorAttribute(string propertyMap)
            : base(propertyMap)
        {}

        protected override void SetCustomAttributes(Control container, HtmlControl controlToDecorate)
        {
            //var parentCategoryId = GetParentCategoryId(container);
            //if (!string.IsNullOrEmpty(parentCategoryId))
            //    controlToDecorate.Attributes["data-parent-category-id"] = parentCategoryId;
        }

        private string GetParentCategoryId(Control container)
        {
            var itemEditor = GetItemEditor(container);
            if (itemEditor == null)
                return null;

            var currentItem = itemEditor.CurrentItem.Parent ?? itemEditor.CurrentItem;

            while (currentItem != null)
            {
                var catalogPage = currentItem as CatalogPage;

                if (catalogPage != null)
                {
                    return catalogPage.CategoryID;
                }

                currentItem = currentItem.Parent;
            }

            return null;
        }

    }
}
