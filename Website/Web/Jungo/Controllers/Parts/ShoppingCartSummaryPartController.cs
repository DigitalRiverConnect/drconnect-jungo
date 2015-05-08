using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Cart;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using ViewModelBuilders.Cart;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{

    [Controls(typeof(ShoppingCartSummaryPart))]
    public class ShoppingCartSummaryPartController : PartControllerBase<ShoppingCartSummaryPart>
    {
        private readonly IShoppingCartViewModelBuilder _shoppingCartViewModelBuilder;

        public ShoppingCartSummaryPartController(IRequestLogger logger, IShoppingCartViewModelBuilder shoppingCartViewModelBuilder)
            : base(logger)
        {
            _shoppingCartViewModelBuilder = shoppingCartViewModelBuilder;
        }

        public override ActionResult Index()
        {
            //todo: figure out if this ought to be cached:
            var mod = WebSessionGet<ShoppingCartViewModel>(WebSession.ShoppingCartSlot) ??
                     _shoppingCartViewModelBuilder.GetShoppingCartViewModelAsync().Result;
            if (mod == null)
            {
                mod = new ShoppingCartViewModel
                {
                    Count = 1,
                    Discount = 1.0m,
                    ShippingAndHandling = 1.0m,
                    ShippingCode = "dummy",
                    ShippingMethods = new ShippingMethod[0],
                    SubTotal = 2.0m,
                    Tax = 3.0m,
                    Total = 4.0m,
                    Cart = new Cart
                    {
                        LineItems = new LineItems
                        {
                            LineItem = new []
                            {
                                new LineItem
                                {
                                    Product = new Product
                                    {
                                        Name = "bad",
                                    },
                                    Quantity = 1
                                }
                            }
                        }
                    },
                    IsShoppingCartLocked = false,
                };
            }
            return PartialView("Index", mod);
        }

    }
}
