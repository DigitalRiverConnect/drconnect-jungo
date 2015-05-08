using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Cart
{
    public class Cart : ResourceLink
    {
        public string Id { get; set; } // this was an int several days ago (such as 14134803982), now it's coming back as a string (such as "active")
        public ResourceLink WebCheckout { get; set; }
        public LineItems LineItems { get; set; }
        public int TotalItemsInCart { get; set; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
        public Payment Payment { get; set; }
        public ShippingMethod ShippingMethod { get; set; }
        public ShippingOptions ShippingOptions { get; set; }
        public CartTotal Pricing { get; set; }
        public CustomAttributes CustomAttributes { get; set; }
    }
}
