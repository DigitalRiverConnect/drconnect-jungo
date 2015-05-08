using System;

namespace Jungo.Models.ShopperApi.Common
{
    public class InventoryStatus : ResourceLink
    {
        public int AvailableQuantity { get; set; }
        public bool AvailableQuantityIsEstimated { get; set; }
        public bool ProductIsInStock { get; set; }
        public bool ProductIsAllowsBackorders { get; set; }
        public bool ProductIsTracked { get; set; }
        public bool RequestedQuantityAvailable { get; set; }
        public string Status { get; set; } //todo: enum?
        public bool StatusIsEstimated { get; set; }
        public DateTime? ExpectedInStockDate { get; set; }
        public string CustomStockMessage { get; set; }
        public ResourceLink[] Product { get; set; }
    }
}
