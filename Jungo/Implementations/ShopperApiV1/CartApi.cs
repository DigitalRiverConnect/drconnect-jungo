using System;
using System.Threading.Tasks;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Cart;
using Jungo.Models.ShopperApi.Common;

namespace Jungo.Implementations.ShopperApiV1
{
    public class CartApi : ICartApi
    {
        #region ICartApi Members

        public async Task<Cart> GetCartAsync()
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    return await InternalGetCartAsync().ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Cart> AddProductToCartAsync(AddProductToCartLink addToCartLink)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    return (await Client.PostAsync<CartResponse>(addToCartLink.CartUri).ConfigureAwait(false)).Cart;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Cart> AddProductToCartAsync(long productId, int quantity = 1, long offerId = 0)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    object parameters;
                    if (quantity > 1 && offerId > 0)
                        parameters = new {productId, quantity, offerId};
                    else if (quantity > 1)
                        parameters = new {productId, quantity};
                    else if (offerId > 0)
                        parameters = new {productId, offerId};
                    else
                        parameters = new {productId};
                    var addProductToCartLink = new AddProductToCartLink
                    {
                        CartUri =
                            Billboard.ResolveTemplate(billboard.Cart, Billboard.Templates.AddProductToCartQuery,
                                parameters)
                    };

                    return await AddProductToCartAsync(addProductToCartLink).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Cart> DeleteLineItemAsync(string lineItemUri)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    await Client.DeleteAsync(lineItemUri).ConfigureAwait(false);
                    return await InternalGetCartAsync().ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Cart> DeleteAllLineItemsAsync()
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var cart = await InternalGetCartAsync().ConfigureAwait(false);
                    await Client.DeleteAsync(cart.LineItems.Uri).ConfigureAwait(false);
                    return await InternalGetCartAsync().ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Cart> ApplyCouponAsync(string coupon)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var applyCouponUri = Billboard.ResolveTemplate(billboard.Cart, Billboard.Templates.ApplyCouponQuery,
                        new {promoCode = coupon, expand = Billboard.Templates.ExpandAllQueryValue});
                    return (await Client.PostAsync<CartResponse>(applyCouponUri).ConfigureAwait(false)).Cart;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Cart> UpdateLineItemQuantityAsync(string lineItemUri, int quantity)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var qtyUri = Billboard.ResolveTemplate(lineItemUri, Billboard.Templates.ChangeQuantityQuery,
                        new {quantity});
                    await Client.PostForInfoAsync(qtyUri).ConfigureAwait(false);
                    return await InternalGetCartAsync().ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Uri> GetWebCheckoutUrlAsync(Cart cart, string themeId)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var uri = (await Client.GetForInfoAsync(cart.WebCheckout.Uri).ConfigureAwait(false)).Location;
                    if (!string.IsNullOrEmpty(themeId))
                    {
                        uri = uri.Query.Length > 0 ? new Uri(uri.AbsoluteUri + "&ThemeId=" + themeId) : new Uri(uri.AbsoluteUri +"?ThemeId=" + themeId);
                    }
                    return uri;
                    //todo: add the themeid so redirect looks pretty on gc
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        #endregion

        #region Implementation

        private async Task<Cart> InternalGetCartAsync()
        {
            var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
            var cartUri = Billboard.ResolveExpandAll(billboard.Cart);
            return (await Client.GetAsync<CartResponse>(cartUri).ConfigureAwait(false)).Cart;
        }

        // Do not make this property static, despite Resharper suggesting you can. To do so would create cross-request problems!!
        private IClient Client { get { return DependencyResolver.Current.Get<IClient>(); } }

        #endregion
    }
}
