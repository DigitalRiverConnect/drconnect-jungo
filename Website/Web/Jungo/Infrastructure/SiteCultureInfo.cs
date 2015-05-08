using System.Runtime.Serialization;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    [DataContract]
    public class SiteCultureInfo
    {
        private readonly string _siteId;
        private readonly string _countryId;
        private readonly string _locale;
        private readonly string _currency;

        public SiteCultureInfo(string siteId, string countryId, string locale, string currency)
        {
            _siteId = siteId;
            _countryId = countryId;
            _locale = locale;
            _currency = currency;
        }

        [DataMember]
        public string SiteId { get { return _siteId; } }

        [DataMember]
        public string CountryId { get { return _countryId; } }

        [DataMember]
        public string Locale { get { return _locale; } }

        [DataMember]
        public string Currency { get { return _currency; } }
    }
}
