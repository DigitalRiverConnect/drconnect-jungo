using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ActionFilters;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Resources;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using Newtonsoft.Json;
using ViewModelBuilders.Cart;
using ViewModelBuilders.Layout;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(ShoppingCartPage))]
    [CheckoutFlow]
    public class ShoppingCartController : ContentControllerBase<ShoppingCartPage>
    {
        public const string ErrMsgResourceTypeErrorCode = "ErrorCode";
        public const string ParamScsErrorCode = "scsCode";
        public const string ErrMsgResourceType = "ShoppingCart";
        public const string ErrMsgAddprodCartLocked = "AddProduct_CartLocked";
        public const string ErrMsgAddprodOutOfStock = "AddProduct_OutOfStock";
        public const string ErrMsgAddprodFailed = "AddProduct_Failed";

        protected readonly ICartApi CartApi;
        protected readonly IShoppingCartViewModelBuilder ShoppingCartViewModelBuilder;
        protected readonly IOffersViewModelBuilder OffersViewModelBuilder;
        protected readonly IPageInfo PageInfo;

        public ShoppingCartController(ICartApi cartApi, IRequestLogger logger, ILinkGenerator linkGenerator,
            IPageInfo pageInfo, IOffersViewModelBuilder offersViewModelBuilder, IShoppingCartViewModelBuilder shoppingCartViewModelBuilder, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            CartApi = cartApi;
            ShoppingCartViewModelBuilder = shoppingCartViewModelBuilder;
            PageInfo = pageInfo;
            OffersViewModelBuilder = offersViewModelBuilder;
        }

        public override ActionResult Index()
        {
            AssertProductsLoaded();
            SetPageTitleSimple();
            var sc = WebSessionGet<ShoppingCartViewModel>(WebSession.ShoppingCartSlot) ??
                     ShoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
            var errorMessages = new List<string>();
            if (!String.IsNullOrEmpty(Request.QueryString[ParamScsErrorCode]))
                errorMessages.Add(GetErrorForScsErrorCode(Request.QueryString[ParamScsErrorCode]));
            if (errorMessages.Any())
                sc.ErrorMessages = errorMessages.ToArray();

            WebSessionSet(WebSession.ShoppingCartSlot, sc);
            SetPageInfo(sc, "Sales.Checkout.ShoppingCart");
            return View("PageTemplates/Default");
        }


        private static string GetErrorForScsErrorCode(string scsCode)
        {
            var scsErrorCode = Convert.ToInt32(scsCode);
            string errorFormat = null;
            switch (scsErrorCode)
            {
                // case 0: ???
                case 3:
                case 1000:
                case 1008:
                case 5560:
                case 10010:
                case 10015:
                case 40575:
                case 85537:
                case 131073:
                    errorFormat = Res.ErrorCode_ErrorProcessingOrderNewBrowser;
                    break;
                case 3000:
                    errorFormat = Res.ErrorCode_ErrorProcessingOrderTooManyItemsInCart;
                    break;
                case 30225:
                case 40185:
                case 75550:
                case 75546:
                    errorFormat = Res.ErrorCode_ErrorProcessingOrderNewBrowserDifferentPayment;
                    break;
                case 10001:
                case 10030:
                case 30145:
                case 40470:
                case 131074:
                case 131079:
                case 131084:
                case 141078:
                    errorFormat = Res.ErrorCode_ErrorProcessingOrder;
                    break;
                case 30001:
                    errorFormat = Res.ErrorCode_ErrorProcessingOrderExpiredCard;
                    break;
            }
            if (String.IsNullOrEmpty(errorFormat))
                return null;

            return String.Format(errorFormat, scsCode);
        }

        [HttpGet]
        public ActionResult Buy(string productIds, int quantity = 1, bool skipInterstitial = false)
        {
            var addProductModels =
                productIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(productId =>
                        new AddProductModel
                        {
                            ProductId = productId,
                            Quantity = quantity.ToString(CultureInfo.InvariantCulture)
                        })
                    .ToArray();
            return InternalAddToCart(addProductModels, skipInterstitial);
        }

        [HttpGet]
        public ActionResult AddToCart(string products, bool skipInterstitial = false, int cpeCode = 0, int scsCode = 0)
        {
            var addProductModels = JsonConvert.DeserializeObject<AddProductModel[]>(products);
            return InternalAddToCart(addProductModels, skipInterstitial, cpeCode, scsCode);
        }

        // virtual for testing only!
        protected virtual ActionResult InternalAddToCart(AddProductModel[] addProductModels, bool skipInterstitial = false, int cpeCode = 0, int scsCode = 0)
        {
            if (addProductModels != null)
            {
                var errorMessages = new List<string>();

                // put the product(s) in the cart
                foreach (var addProductModel in addProductModels)
                {
                    try
                    {
                        var cart = CartApi.AddProductToCartAsync(Convert.ToInt64(addProductModel.ProductId), Convert.ToInt32(addProductModel.Quantity),
                            Convert.ToInt64(addProductModel.OfferId)).Result;
                        WebSession.Current.SetPersistentProperty(WebSession.ShoppingCartCount, cart.TotalItemsInCart.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception exc)
                    {
                        var tempErrorMessages = new List<string>();
                        var shopperApiException = exc as ShopperApiException;
                        if (shopperApiException != null)
                        {
                            switch (shopperApiException.ShopperApiError.Code)
                            {
                                case "inventory-unavailable-error":
                                    tempErrorMessages.Add(Res.ShoppingCart_AddProductOutOfStock);
                                    break;
                                default:
                                    tempErrorMessages.Add(Res.ShoppingCart_AddProductFailed);
                                    break;
                            }
                        }
                        else
                            tempErrorMessages.Add(Res.ShoppingCart_AddProductFailed);
                        errorMessages.AddRange(tempErrorMessages);
                    }
                }
                if (errorMessages.Any())
                {
                    var model = ShoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
                    model.ErrorMessages = errorMessages.ToArray();
                    WebSessionSet(WebSession.ShoppingCartSlot, model);
                    return Index();
                }

                if (addProductModels.Length == 1 && !skipInterstitial)
                {
                    var productId = int.Parse(addProductModels[0].ProductId);
                    var interstitialLink = LinkGenerator.GenerateInterstitialLink(productId);
                    if (!String.IsNullOrEmpty(interstitialLink))
                        return Redirect(interstitialLink);

                    var siteRoot = CmsFinder.FindSitePageFromSiteId(WebSession.Current.SiteId);
                    if (siteRoot != null && !String.IsNullOrEmpty(siteRoot.PromotionId))
                    {
                        var offers = OffersViewModelBuilder.GetCrossSellViewModelAsync(siteRoot.PromotionId,
                            CatalogApi.GetProductUri(productId), LinkGenerator.GenerateShoppingCartLink()).Result;
                        if (offers != null && offers.Offers.Count > 0)
                        {
                            interstitialLink = LinkGenerator.GenerateInterstitialLink();
                            if (!String.IsNullOrEmpty(interstitialLink))
                                return Redirect(interstitialLink + "/" + productId);
                        }
                    }
                }
            }
            var cartLink = LinkGenerator.GenerateShoppingCartLink();
            if (scsCode != 0)
            {
                cartLink +=
                    (cartLink.LastIndexOf('?') == -1 ? "?" : "&") + ParamScsErrorCode + "=" + scsCode;
            }
            return Redirect(cartLink);
        }

        protected void SetPageInfo(ShoppingCartViewModel sc, string pageName)
        {
            PageInfo.PageName = pageName;
            PageInfo.CartTotal = sc.SubTotal;
            PageInfo.ConversionStage = 1;
            PageInfo.ErrorCount = sc.ErrorMessages != null ? sc.ErrorMessages.Length : 0;
        }

        protected override void ProcessOutputCache(ResultExecutingContext filterContext)
        {
            // We do not want to cache anything on this controller, so do not cache.
        }
    }
}