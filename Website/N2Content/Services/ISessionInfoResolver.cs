namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services
{
    public interface ISessionInfoResolver
    {
        string SiteId { get; }

        string CultureCode { get; }

        bool IsInitialized { get; }

        string GetSiteMarketPlaceName();
    }
}
