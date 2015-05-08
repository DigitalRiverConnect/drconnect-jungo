using System.Web.Mvc;
using Jungo.Api;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    public class SearchController : Controller
    {
        private readonly ICatalogApi _catalogApi;

        public SearchController(ICatalogApi catalogApi)
        {
            _catalogApi = catalogApi;
        }

        [HttpGet]
        public JsonResult AutoComplete()
        {
            return Json(_catalogApi.GetProductBriefsAsync().Result, JsonRequestBehavior.AllowGet);
        }
    }
}
