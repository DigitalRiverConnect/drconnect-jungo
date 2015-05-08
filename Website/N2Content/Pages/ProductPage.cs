using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Product Page",
        Description = "A product detail page.",
        SortOrder = 220,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/product16.png")]
    [WithEditableName]
    public class ProductPage : CachingPageBase, IProductPart
    {
        [EditableProductMultiSelection("Id:ProductID; Title:Title;", Title = "Product List", SortOrder = 101)]
        public virtual string ProductID
        {
            get { return (string) (GetDetail("ProductID") ?? string.Empty); }
            set { SetDetail("ProductID", value, string.Empty); }
        }

        public string Product
        {
            get { return ProductID; }
            set { ProductID = value; }
        }

        [EditableDropDownAfterAddToCart(Title = "After Add-to-cart", SortOrder = 102)]
        public virtual string AfterAddToCart
        {
            get { return (string)(GetDetail("AfterAddToCart") ?? "0"); }
            set { SetDetail("AfterAddToCart", value, "0"); }
        }

        public AfterAddToCartOption AfterAddToCartOption
        {
            get
            {
                var opt = AfterAddToCart;
                return string.IsNullOrEmpty(opt)
                           ? AfterAddToCartOption.GoToShoppingCart
                           : (AfterAddToCartOption)int.Parse(opt);
            }
        }
    }
}