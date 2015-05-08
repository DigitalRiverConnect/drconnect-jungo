using System;
using System.Threading.Tasks;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using Jungo.Api;

namespace ViewModelBuilders.Cart
{
    public interface IShoppingCartViewModelBuilder
    {
        Task<ShoppingCartViewModel> GetShoppingCartViewModelAsync();
    }

    public class ShoppingCartViewModelBuilder : IShoppingCartViewModelBuilder
    {
        private readonly ICartApi _cartApi;

        public ShoppingCartViewModelBuilder(ICartApi cartApi)
        {
            _cartApi = cartApi;
        }

        public async Task<ShoppingCartViewModel> GetShoppingCartViewModelAsync()
        {
            Jungo.Models.ShopperApi.Cart.Cart cart;
            try
            {
                cart = await _cartApi.GetCartAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                cart = new Jungo.Models.ShopperApi.Cart.Cart { TotalItemsInCart = 0 };
            }
            return MakeShoppingCartViewModel(cart);
        }

        #region Private Methods

        // public for testing only
        public ShoppingCartViewModel MakeShoppingCartViewModel(Jungo.Models.ShopperApi.Cart.Cart cart)
        {
            return new ShoppingCartViewModel
            {
                Count = cart.TotalItemsInCart,
                SubTotal = cart.Pricing.Subtotal != null ? cart.Pricing.Subtotal.Value : 0.0M,
                Discount = cart.Pricing.Discount != null ? cart.Pricing.Discount.Value : 0.0M,
                ShippingAndHandling = cart.Pricing.ShippingAndHandling != null ? cart.Pricing.ShippingAndHandling.Value : 0.0M,
                Tax = cart.Pricing.Tax != null ? cart.Pricing.Tax.Value : 0.0M,
                Total = cart.Pricing.OrderTotal != null ? cart.Pricing.OrderTotal.Value : 0.0M,
                Cart = cart,
                IsShoppingCartLocked = false,
            };
        }

        #endregion
    }
}
