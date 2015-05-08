using System;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    public class ErrorsController : Controller
    {
        private readonly ILinkGenerator _linkGenerator;

        public ErrorsController(ILinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public ActionResult Http404()
        {
            string storeLink;

            try
            {
                storeLink = _linkGenerator.GenerateStoreLink();
            }
            catch (Exception)
            {
                storeLink = "http://www.MicrosoftStore.com";
            }

            return View("404", (object)storeLink);
        }

        public ActionResult Http500()
        {
            Exception ex = null;
#if DEBUG
            if (RouteData.Values.ContainsKey("exception"))
            {
                ex = (Exception)RouteData.Values["exception"];
            }
#endif
            return View("500", ex);
        }
    }
}
