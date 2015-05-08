namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services
{
    public interface ILinkGenerator
    {
        string GenerateCategoryLink(long? categoryId = null, bool? forceListPage = null);
        string GenerateCategoryLink(long? categoryId, bool? forceListPage, out object matchingCatalogPage);
        string GenerateProductLink(long? productId = null);
        string GenerateProductLink(long? productId, out object matchingProductPage);
        string GenerateInterstitialLink();
        string GenerateInterstitialLink(long? productId);
        string GenerateShoppingCartLink();
        string GenerateSearchActionLink();
        string GenerateStoreLink();
        string GenerateSearchLink();
        string GenerateFAQLink();
        //string GenerateNotFoundLink();
        //string GenerateServerErrorLink();
        string GenerateLinkForNamedContentItem(string name);
    }
}