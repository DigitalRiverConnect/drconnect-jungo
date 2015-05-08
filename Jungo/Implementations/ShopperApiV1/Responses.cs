using Jungo.Models.ShopperApi.Cart;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;
using Jungo.Models.ShopperApi.Offers;

namespace Jungo.Implementations.ShopperApiV1
{
    internal class CartResponse
    {
        public Cart Cart { get; set; }
    }

    internal class CategoriesResponse
    {
        public Category Category { get; set; }
    }

    internal class OffersResponse
    {
        public Offers Offers { get; set; }
    }

    internal class OfferResponse
    {
        public Offer Offer { get; set; }
    }

    internal class ProductsResponse
    {
        public Products Products { get; set; }
    }

    internal class ProductsWithRankingResponse
    {
        public ProductsWithRanking Products { get; set; }
    }

    internal class ProductResponse
    {
        public Models.ShopperApi.Catalog.Product Product { get; set; }
    }

    internal class PointOfPromotionsResponse
    {
        public PointOfPromotions PointOfPromotions { get; set; }
    }

    internal class PointOfPromotionResponse
    {
        public PointOfPromotion PointOfPromotion { get; set; }
    }

    internal class ProductOffersResponse
    {
        public ProductOffers ProductOffers { get; set; }
    }

    internal class ErrorsResponse
    {
        public Errors Errors { get; set; }
    }

    internal class Errors
    {
        public Error Error { get; set; }
    }

    internal class InventoryStatusResponse
    {
        public InventoryStatus InventoryStatus { get; set; }
    }
}
