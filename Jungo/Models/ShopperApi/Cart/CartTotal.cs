using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Cart
{
    public class CartTotal
    {
        public MoneyAmount Subtotal { get; set; }
        public MoneyAmount Discount { get; set; }
        public MoneyAmount ShippingAndHandling { get; set; }
        public MoneyAmount Tax { get; set; }
        public MoneyAmount OrderTotal { get; set; }
        public string FormattedSubtotal { get; set; }
        public string FormattedDiscount { get; set; }
        public string FormattedShippingAndHandling { get; set; }
        public string FormattedTax { get; set; }
        public string FormattedOrderTotal { get; set; }
    }
}
