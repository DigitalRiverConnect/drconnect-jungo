using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Models.ShopperApi.Common;
using Jungo.Models.ShopperApi.Offers;
using Omu.ValueInjecter;

namespace ViewModelBuilders.Layout
{
    public interface IOffersViewModelBuilder
    {
        Task<CrossSellViewModel> GetCrossSellViewModelAsync(string promotionId, string shoppingCartLink);
        Task<CrossSellViewModel> GetCrossSellViewModelAsync(string promotionId, ResourceUri productUri, string shoppingCartLink);
    }

    public class OffersViewModelBuilder : IOffersViewModelBuilder
    {
        private readonly IOffersApi _offersApi;
        private readonly ILinkGenerator _linkGenerator;

        public OffersViewModelBuilder(IOffersApi offersApi, ILinkGenerator linkGenerator)
        {
            _offersApi = offersApi;
            _linkGenerator = linkGenerator;
        }

        public async Task<CrossSellViewModel> GetCrossSellViewModelAsync(string promotionId, string shoppingCartLink)
        {
            var pop = await _offersApi.GetOffersAsync(promotionId).ConfigureAwait(false);
            return MakeCrossSellViewModel(pop, _linkGenerator.GenerateShoppingCartLink());
        }

        public async Task<CrossSellViewModel> GetCrossSellViewModelAsync(string promotionId, ResourceUri productUri, string shoppingCartLink)
        {
            var pop = await _offersApi.GetOffersAsync(promotionId, productUri).ConfigureAwait(false);
            return MakeCrossSellViewModel(pop, shoppingCartLink);
        }

        public static void RemoveCartItemsFromOffers(CrossSellViewModel vm, Jungo.Models.ShopperApi.Cart.Cart cart)
        {
            // remove what is already in the cart from the cross-sell offers
            if (vm == null || vm.Offers == null || cart == null || cart.LineItems == null ||
                cart.LineItems.LineItem == null || cart.LineItems.LineItem.Length <= 0) return;
            foreach (var offer in vm.Offers.Where(offer => offer.ProductOffersOfferViewModels != null && offer.ProductOffersOfferViewModels.Length > 0))
            {
                offer.ProductOffersOfferViewModels =
                    offer.ProductOffersOfferViewModels.Where(
                        povm => cart.LineItems.LineItem.All(li => li.Product.Id != povm.Product.Id))
                        .ToArray();
            }
        }

        private CrossSellViewModel MakeCrossSellViewModel(Offers offers, string shoppingCartLink)
        {
            if (offers == null || offers.Offer == null || offers.Offer.Length == 0)
                return null;
            var offerViewModels = new List<CrossSellOfferViewModel>();
            foreach (var offer in offers.Offer)
            {
                var offerViewModel = OfferToCrossSellOfferViewModel(offer);
                offerViewModels.Add(offerViewModel);
                if (offerViewModel.ProductOffersOfferViewModels == null || offerViewModel.ProductOffersOfferViewModels.Length == 0) continue;
                foreach (var productOfferViewModel in offerViewModel.ProductOffersOfferViewModels)
                {
                    productOfferViewModel.AddToCartLink = MakeAddToCartLink(productOfferViewModel.Product.Id, offer.Id, shoppingCartLink);
                    productOfferViewModel.ProductLink =
                        _linkGenerator.GenerateProductLink(productOfferViewModel.Product.Id);
                }
            }
            return new CrossSellViewModel
            {
                Offers = offerViewModels
            };
        }

        private static CrossSellOfferViewModel OfferToCrossSellOfferViewModel(Offer offer)
        {
            var offerViewModel = new CrossSellOfferViewModel();
            offerViewModel.InjectFrom(offer);
            if (offer.ProductOffers.ProductOffer != null && offer.ProductOffers.ProductOffer.Length > 0)
            {
                offerViewModel.ProductOffersOfferViewModels = offer.ProductOffers.ProductOffer.Select(po =>
                {
                    var povm = new ProductOfferViewModel();
                    povm.InjectFrom(po);
                    return povm;
                }).ToArray();
            }
            return offerViewModel;
        }

        private string MakeAddToCartLink(long productId, long offerId, string shoppingCartLink)
        {
            if (String.IsNullOrEmpty(shoppingCartLink)) return shoppingCartLink;
            if (!shoppingCartLink.EndsWith("/"))
                shoppingCartLink += "/";
            var json = "[{\"ProductId\":\"" + productId + "\",\"Quantity\":1,\"OfferId\":\"" + offerId + "\"}]";
            shoppingCartLink += "addtocart?products=" + HttpUtility.UrlEncode(json);
            return shoppingCartLink;
        }
    }
}
