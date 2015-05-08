using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Models.ShopperApi.Offers;
using Omu.ValueInjecter;

namespace ViewModelBuilders.Catalog
{
    public interface IOfferListViewModelBuilder
    {
        Task<OfferListViewModel> GetOfferListAsync(OfferListPart offerListPart);
        Task<OfferListViewModel> GetProductOfferListAsync(ProductOfferListPart productOfferListPart, IProductPart page, ICatalogApi catalogApi);
    }

    public class OfferListViewModelBuilder : IOfferListViewModelBuilder
    {
        private readonly IOffersApi _offersApi;
        private readonly ILinkGenerator _linkGenerator;

        public OfferListViewModelBuilder(IOffersApi offersApi, ILinkGenerator linkGenerator)
        {
            _offersApi = offersApi;
            _linkGenerator = linkGenerator;
        }

        public async Task<OfferListViewModel> GetOfferListAsync(OfferListPart offerListPart)
        {
            if (String.IsNullOrEmpty(offerListPart.PopName))
                return new OfferListViewModel();
            var offers = await _offersApi.GetOffersAsync(offerListPart.PopName).ConfigureAwait(false);
            return OffersToOfferListViewModel(offerListPart, offers);
        }

        public async Task<OfferListViewModel> GetProductOfferListAsync(ProductOfferListPart productOfferListPart, IProductPart page, ICatalogApi catalogApi)
        {
            // The driving product is taken off of either the product on the page (if the page is a product page) or off of the "Product" picker on the part.
            // The Product picker will override the page's product.
            long pid;
            if (!long.TryParse(productOfferListPart.Product, out pid) || pid == 0)
            {
                if (page != null && page.Product.Contains(","))
                {
                    var parts = page.Product.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        long.TryParse(parts[0], out pid);
                }
            }
            if (pid == 0)
                return await GetOfferListAsync(productOfferListPart).ConfigureAwait(false);
            if (String.IsNullOrEmpty(productOfferListPart.PopName))
                return new OfferListViewModel();
            var offers = await _offersApi.GetOffersAsync(productOfferListPart.PopName, catalogApi.GetProductUri(pid)).ConfigureAwait(false);
            return OffersToOfferListViewModel(productOfferListPart, offers);
        }

        #region implementation

        private OfferListViewModel OffersToOfferListViewModel(OfferListPart offerListPart, Offers offers)
        {
            var offerListViewModel = new OfferListViewModel { Title = offerListPart.Title };
            if (offers == null || offers.Offer == null)
                return offerListViewModel;

            var offer = offers.Offer.FirstOrDefault(o => o.ProductOffers != null && o.ProductOffers.ProductOffer != null && o.ProductOffers.ProductOffer.Length > 0) ??
                        offers.Offer[0];
            offerListViewModel.Title = offerListPart.Title;
            offerListViewModel.Id = offer.Id;
            offerListViewModel.Name = offer.Name;
            offerListViewModel.Type = offer.Type;
            offerListViewModel.Image = offer.Image;
            offerListViewModel.SalesPitch = offer.SalesPitch;
            if (offer.ProductOffers == null || offer.ProductOffers.ProductOffer == null)
                return offerListViewModel;

            if (offer.ProductOffers.ProductOffer.Length > offerListPart.MaxNProducts)
            {
                var limitedProductOffers = new ProductOffer[offerListPart.MaxNProducts];
                for (var i = 0; i < offerListPart.MaxNProducts; i++)
                    limitedProductOffers[i] = offer.ProductOffers.ProductOffer[i];
                offerListViewModel.ProductOfferViewModels = limitedProductOffers.Select(po => ProductOfferToProductOfferViewModel(offer.Id, po)).ToArray();
            }
            else
                offerListViewModel.ProductOfferViewModels = offer.ProductOffers.ProductOffer.Select(po => ProductOfferToProductOfferViewModel(offer.Id, po)).ToArray();
            return offerListViewModel;
        }

        private ProductOfferViewModel ProductOfferToProductOfferViewModel(long offerId, ProductOffer productOffer)
        {
            var productOfferViewModel = new ProductOfferViewModel();
            productOfferViewModel.InjectFrom(productOffer);
            if (productOffer.Product != null)
            {
                productOfferViewModel.ProductLink = _linkGenerator.GenerateProductLink(productOffer.Product.Id);
                productOfferViewModel.AddToCartLink = MakeAddToCartLink(productOffer.Product.Id, offerId);
            }
            return productOfferViewModel;
        }

        private string MakeAddToCartLink(long productId, long offerId)
        {
            var link = _linkGenerator.GenerateShoppingCartLink() ?? "";
            if (String.IsNullOrEmpty(link)) return link;
            if (!link.EndsWith("/"))
                link += "/";
            var json = "[{\"ProductId\":\"" + productId + "\",\"Quantity\":1,\"OfferId\":\"" + offerId + "\"}]";
            link += "addtocart?products=" + HttpUtility.UrlEncode(json);
            return link;
        }

        #endregion
    }
}
