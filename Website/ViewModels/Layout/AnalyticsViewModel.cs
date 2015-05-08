using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class AnalyticsViewModel
    {
        // properties for every page
        public string PageTitle { get; set; }
        public string Locale { get; set; }
        public string SiteId { get; set; }
        public string LanguageCode { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public IDictionary<string, string> PageInfo { get; set; }
    }
}
