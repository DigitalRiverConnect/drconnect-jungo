using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Attributes;
using Jungo.Models.ShopperApi.Cart;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart
{
    public class ShoppingCartViewModel
    {
        [JsonProperty(Name = "count")]
        public int Count { get; set; }

        [JsonProperty(Name = "subTotal")]
        public decimal SubTotal { get; set; }

        [JsonProperty(Name = "discount")]
        public decimal Discount { get; set; }

        [JsonProperty(Name = "shippingAndHandling")]
        public decimal ShippingAndHandling { get; set; }

        [JsonProperty(Name = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(Name = "total")]
        public decimal Total { get; set; }

        public Jungo.Models.ShopperApi.Cart.Cart Cart { get; set; }

        public ShippingMethod[] ShippingMethods { get; set; }

        public string ShippingCode { get; set; }

        public string[] ErrorMessages { get; set; }

        public bool IsShoppingCartLocked { get; set; }
    }

}
