using System;
using System.Threading.Tasks;
using Jungo.Models.ShopperApi.Cart;
using Jungo.Models.ShopperApi.Common;

namespace Jungo.Api
{
    public interface ICartApi
    {
        /// <summary>
        /// </summary>
        /// <returns>null if none; cart contains web-checkout link</returns>
        Task<Cart> GetCartAsync();

        /// <summary>
        /// adds a product to the cart
        /// </summary>
        /// <param name="addToCartLink">add-to-cart link for desired product</param>
        /// <returns>cart with newly added line item and other lines; cart contains web-checkout link</returns>
        Task<Cart> AddProductToCartAsync(AddProductToCartLink addToCartLink);

        /// <summary>
        /// adds a product to the cart
        /// </summary>
        /// <param name="productId">pid for desired product</param>
        /// <param name="quantity">quantity desired</param>
        /// <param name="offerId">an offerId if the product is being added to the cart in the context of an offer</param>
        /// <returns>cart with newly added line item and other lines; cart contains web-checkout link</returns>
        Task<Cart> AddProductToCartAsync(long productId, int quantity = 1, long offerId = 0);

        Task<Cart> DeleteLineItemAsync(string lineItemUri);
        Task<Cart> DeleteAllLineItemsAsync();
        Task<Cart> ApplyCouponAsync(string coupon);
        Task<Cart> UpdateLineItemQuantityAsync(string lineItemUri, int quantity);

        /// <summary>
        /// get URL to redirect browser to for starting the Web Checkout flow at DR
        /// </summary>
        /// <returns>web checkout redirect URL</returns>
        Task<Uri> GetWebCheckoutUrlAsync(Cart cart, string themeId);
    }
}
