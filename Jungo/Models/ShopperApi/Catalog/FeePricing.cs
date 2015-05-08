using Jungo.Models.ShopperApi.Common;

namespace Jungo.Models.ShopperApi.Catalog
{
    public class FeePricing
    {
        public MoneyAmount SalePriceWithFeesAndQuantity { get; set; }
        public string FormattedSalePriceWithFeesAndQuantity { get; set; }
    }
}
