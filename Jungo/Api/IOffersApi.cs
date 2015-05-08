using System.Collections.Generic;
using System.Threading.Tasks;
using Jungo.Models.ShopperApi.Common;
using Jungo.Models.ShopperApi.Offers;

namespace Jungo.Api
{
    /// <summary>
    /// In general, Gets return null if not found
    /// </summary>
    public interface IOffersApi
    {
        Task<Offers> GetOffersAsync();

        Task<Offers> GetOffersAsync(string popName);
        Task<Offers> GetOffersAsync(ResourceUri productUri);
        Task<Offers> GetOffersAsync(string popName, ResourceUri productUri);
        Task<Offers> GetOffersForCartAsync(string popName); // uses GetOffersByPop internally and weeds out what's in the cart already

        Task<IEnumerable<string>> GetPointOfPromotionNamesAsync();
        Task<PointOfPromotion> GetPointOfPromotionAsync(string popName);
    }
}
