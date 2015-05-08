namespace Jungo.Models.ShopperApi.Common
{
    public class Pricing
    {
        public MoneyAmount ListPrice { get; set; }
        public MoneyAmount ListPriceWithQuantity { get; set; }
        public MoneyAmount SalePriceWithQuantity { get; set; }
        public string FormattedListPrice { get; set; }
        public string FormattedListPriceWithQuantity { get; set; }
        public string FormattedSalePriceWithQuantity { get; set; }
    }

    public class PricingWithDiscount : Pricing
    {
        public MoneyAmount TotalDiscountWithQuantity { get; set; }
        public string DiscountDescription { get; set; }
    }
}
