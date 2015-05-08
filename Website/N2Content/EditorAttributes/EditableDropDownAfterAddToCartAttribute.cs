using System.Globalization;
using System.Web.UI.WebControls;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2.Details;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public class EditableDropDownAfterAddToCartAttribute : EditableDropDownAttribute
    {
        protected override ListItem[] GetListItems()
        {
            return new[]
                {
                    new ListItem {Text = "Go to Shopping Cart page", Value = ((int)AfterAddToCartOption.GoToShoppingCart).ToString(CultureInfo.InvariantCulture)},
                    new ListItem {Text = "Stay on this product page", Value = ((int)AfterAddToCartOption.StayOnProductPage).ToString(CultureInfo.InvariantCulture)},
                };
        }
    }
}