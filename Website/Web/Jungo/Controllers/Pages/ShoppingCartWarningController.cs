using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(ShoppingCartWarningPage))]
    public class ShoppingCartWarningController : ContentControllerBase<ShoppingCartWarningPage>
    {
        public ShoppingCartWarningController(IRequestLogger logger, ILinkGenerator linkGenerator,
            ICatalogApi catalogApi) : base(logger, linkGenerator, catalogApi)
        {
        }

        public override ActionResult Index()
        {
            AssertProductsLoaded();
            return View("Index", new WarningErrorModel{ErrorMessage = string.Empty});
        }
    }
}