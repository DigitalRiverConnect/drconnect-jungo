using System;

namespace Jungo.Infrastructure.Config.Models
{
    [Serializable]
    public class SiteInfo
    {
        public string CompanyId { get; set; }
        public string SiteId { get; set; }
        public string Country { get; set; }
        public string Locale { get; set; }
        public string GcLocale { get; set; }
        public string Currency { get; set; }
        public bool IsDefault { get; set; }
        public bool IsCartHandledBySite { get; set; }
        public string DrBuyNowFormAction { get; set; }
        public string DrShoppingCartUrl { get; set; }
        public string DrThemeId { get; set; }
        public string MarketPlaceName { get; set; }
        public bool IsDefaultMarketPlace { get; set; }
    }
}