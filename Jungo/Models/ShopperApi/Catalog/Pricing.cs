using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class Pricing : PricingWithDiscount
    {
        public FeePricing FeePricing { get; set; }
        public bool ListPriceIncludesTax { get; set; }
        public MoneyAmount MsrpPrice { get; set; }
        public string FormattedMsrpPrice { get; set; }
    }
}
