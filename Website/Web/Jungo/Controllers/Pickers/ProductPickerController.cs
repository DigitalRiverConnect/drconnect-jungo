using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Jungo.Api;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pickers
{
    [Authorize]
    public class ProductPickerController : Controller
    {
        private readonly ICatalogApi _catalogApi;

        public enum ProductPickerMode
        {
            Single = 1,
            Multi = 2
        }

        public ProductPickerController(ICatalogApi catalogApi)
        {
            _catalogApi = catalogApi;
        }

        //
        // GET: /ProductPicker/

        [HttpGet]
        public ActionResult Index(string id, ProductPickerMode mode, string selectedItems)
        {
            var viewModel = new ProductPickerViewModel
            {
                SearchCriteria = "",
                Mode = mode.ToString(),
                ControlId = id,
                SelectedItems = ConvertSelectedItemsToJson(selectedItems)
            };

            return View(viewModel);
        }

        [HttpGet]
        public JsonResult SearchProducts(string keywords, string page = "1", string pageSize = "15")
        {
            var searchOptions = SearchOptionsUtils.GetPagingOptions(string.IsNullOrEmpty(page) || page == "0" ? "1" : page, pageSize);

            var result = _catalogApi.GetProductsByKeywordAsync(string.Format("{0}", keywords), searchOptions).Result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private string ConvertSelectedItemsToJson(string selectedItems)
        {
            var sia = string.IsNullOrEmpty(selectedItems) ? new string[0] : selectedItems.Split(',');
            var currentContext = System.Web.HttpContext.Current;

            var products = sia.AsParallel().Select(pid =>
            {
                long prodId;
                if (!long.TryParse(pid, out prodId))
                    return null;

                System.Web.HttpContext.Current = currentContext;
                var p = _catalogApi.GetProductAsync(_catalogApi.GetProductUri(prodId)).Result;
                if (p == null)
                    return null;
                return new ProductEntityViewModel
                    {
                        Id = p.Id,
                        Title = p.DisplayName,
                        Description = p.ShortDescription
                    };
            }).Where(product => product != null).ToList();

            var serializer = new DataContractJsonSerializer(typeof(ProductEntityViewModel[]));
            var ms = new MemoryStream();
            serializer.WriteObject(ms, products.ToArray());
            var json = ms.ToArray();
            ms.Close();

            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
    }


}
