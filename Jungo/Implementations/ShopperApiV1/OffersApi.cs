using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Offers;
using Jungo.Models.ShopperApi.Common;

namespace Jungo.Implementations.ShopperApiV1
{
    public class OffersApi : IOffersApi
    {
        #region IOffersApi Members

        public async Task<Offers> GetOffersAsync()
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var offersUri = Billboard.ResolveExpandAll(billboard.Offers);
                    var offers = await InternalGetOffersForOfferUri(offersUri).ConfigureAwait(false);

                    return offers;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Offers> GetOffersAsync(string popName)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var pop = await InternalGetPointOfPromotionAsync(popName).ConfigureAwait(false);
                    if (pop == null || pop.Offers == null || String.IsNullOrEmpty(pop.Offers.Uri))
                        return null;
                    var offersUri = Billboard.ResolveExpandAll(pop.Offers.Uri);
                    return await InternalGetOffersForOfferUri(offersUri).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Offers> GetOffersAsync(ResourceUri productUri)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var prodOffersUri =
                        Billboard.ResolveExpandAll(productUri.Uri + Billboard.Templates.ProductOffersSegment);
                    return await InternalGetOffersForOfferUri(prodOffersUri).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Offers> GetOffersAsync(string popName, ResourceUri productUri)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var prodPopOffersUri = Billboard.ResolveExpandAll(
                        Billboard.ResolveTemplate(productUri.Uri, Billboard.Templates.PopOffersSegment,
                            new {pop = popName}));
                    return await InternalGetOffersForOfferUri(prodPopOffersUri).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Offers> GetOffersForCartAsync(string popName)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                OffersResponse offersResponse;

                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var cartPopOffersUri = Billboard.ResolveTemplate(billboard.Cart,
                        Billboard.Templates.PopOffersSegment,
                        new {pop = popName});
                    // hitting cart resource for offers returns baby offers, even if use expand=all
                    offersResponse =
                        (await Client.GetCacheableAsync<OffersResponse>(cartPopOffersUri).ConfigureAwait(false));
                    if (offersResponse == null || offersResponse.Offers == null || offersResponse.Offers.Offer == null)
                        return null;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }

                var offers = offersResponse.Offers;
                // expand each baby offer to papa offer in parallel
                var offerGet = new Task<OfferResponse>[offers.Offer.Length];
                for (var idx = 0; idx < offers.Offer.Length; idx++)
                    offerGet[idx] =
                        Client.GetCacheableAsync<OfferResponse>(Billboard.ResolveExpandAll(offers.Offer[idx].Uri));

                var exceptions = new List<Exception>();
                // wait for all
                for (var idx = 0; idx < offerGet.Length; idx++)
                {
                    try
                    {
                        var offerResponse = await offerGet[idx].ConfigureAwait(false);
                        if (offerResponse != null)
                        {
                            offers.Offer[idx] = offerResponse.Offer;
                            await SupplyRecentInventoryStatusAsync(offers.Offer[idx]).ConfigureAwait(false);
                        }
                    }
                    catch (Exception exc)
                    {
                        exceptions.Add(exc);
                    }
                }
                if (exceptions.Any())
                    throw log.LogException(new AggregateException(exceptions));
                return offers;
            }
        }

        public async Task<IEnumerable<string>> GetPointOfPromotionNamesAsync()
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var popsResponse =
                        (await Client.GetCacheableAsync<PointOfPromotionsResponse>(billboard.PointOfPromotions)
                            .ConfigureAwait(false));
                    return popsResponse == null || popsResponse.PointOfPromotions == null ||
                                   popsResponse.PointOfPromotions.PointOfPromotion == null
                        ? new string[0]
                        : popsResponse.PointOfPromotions.PointOfPromotion.Select(p => p.Id);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<PointOfPromotion> GetPointOfPromotionAsync(string popName)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    return await InternalGetPointOfPromotionAsync(popName).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        #endregion

        #region implementation

        private async Task<Offers> InternalGetOffersForOfferUri(string uri)
        {
            var offersResponse = await Client.GetCacheableAsync<OffersResponse>(uri).ConfigureAwait(false);
            if (offersResponse == null || offersResponse.Offers == null || offersResponse.Offers.Offer == null) return null;
            var offers = offersResponse.Offers;
            // don't do all offers in parallel -- it could blast tons of requests out -- just do one batch of prods at a time
            foreach (var offer in offers.Offer)
                await SupplyRecentInventoryStatusAsync(offer).ConfigureAwait(false);
            return offers;
        }

        private async Task SupplyRecentInventoryStatusAsync(Offer offer)
        {
            if (offer == null || offer.ProductOffers == null || offer.ProductOffers.ProductOffer == null ||
                offer.ProductOffers.ProductOffer.Length == 0)
                return;
            var prods = offer.ProductOffers.ProductOffer.Select(prodOffer => prodOffer.Product).ToList();
            var catApi = new CatalogApi();
            await catApi.SupplyRecentInventoryStatusAsync(prods).ConfigureAwait(false);
        }

        private async Task<PointOfPromotion> InternalGetPointOfPromotionAsync(string popName)
        {
            var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
            var popUri = Billboard.ResolveTemplate(billboard.PointOfPromotions, Billboard.Templates.IdSegment,
                new { id = popName });
            PointOfPromotion pop = null;
            var popResponse = (await Client.GetCacheableAsync<PointOfPromotionResponse>(popUri).ConfigureAwait(false));
            if (popResponse != null)
                pop = popResponse.PointOfPromotion;
            return pop;
        }

        // Do not make this property static, despite Resharper suggesting you can. To do so would create cross-request problems!!
        private IClient Client { get { return DependencyResolver.Current.Get<IClient>(); } }

        #endregion
    }
}
