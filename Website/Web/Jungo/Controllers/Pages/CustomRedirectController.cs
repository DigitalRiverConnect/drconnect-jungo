using System;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof (CustomRedirectPage))]
    public class CustomRedirectController : ContentControllerBase<CustomRedirectPage>
    {
        private readonly ILinkGenerator _linkGenerator;

        public CustomRedirectController(ILinkGenerator linkGenerator, ICatalogApi catalogApi)
            : base(catalogApi)
        {
            _linkGenerator = linkGenerator;
        }

        private const string AdminDisplayHtml = "<html><body>redirect to <a href='{0}' target='_blank'>{0}</a></body></html>";

        public override ActionResult Index()
        {
            if (CurrentItem == null)
                throw new Exception();

            AssertProductsLoaded();

            var redirectUrl = CurrentItem.GetUrl(_linkGenerator);

            if (IsManaging)
                return Content(string.Format(AdminDisplayHtml, redirectUrl + Request.Url.Query ?? "(unspecified)"),
                    "text/html; charset=UTF-8");

            return string.IsNullOrEmpty(redirectUrl)
                ? Redirect(_linkGenerator.GenerateStoreLink())
                : RedirectPermanent(redirectUrl + Request.Url.Query);
        }
    }
}
