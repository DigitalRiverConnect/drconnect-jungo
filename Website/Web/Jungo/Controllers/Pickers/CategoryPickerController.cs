using System.Threading.Tasks;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pickers
{
    public class CategoryPickerController : Controller
    {
        private readonly ICategoryViewModelBuilder _catViewModelBuilder;

        public CategoryPickerController(ICategoryViewModelBuilder catViewModelBuilder)
        {
            _catViewModelBuilder = catViewModelBuilder;
        }

        //
        // GET: /CategoryPicker/

        [HttpGet]
        public async Task<ActionResult> Index(string id, string categoryId)
        {
            long? catId;
            try
            {
                catId = long.Parse(categoryId);
            }
            catch
            {
                catId = null;
            }
            
            var categories = await _catViewModelBuilder.GetCategoriesAsync(catId, levels: 99).ConfigureAwait(false);

            var viewModel = new CategoryPickerViewModel
            {
                SearchCriteria = "",
                Categories = categories,
                ControlId = id
            };

            return View(viewModel);
        }

        //[HttpPost]
        //public ActionResult Index(CategoryPickerViewModel request)
        //{
        //    var categories = _catViewModelBuilder.GetCategoriesAsync(null);  // ToDo:  EG SearchCategories(request.SearchCriteria)
        //    request.Categories = categories;
        //    return View(request);
        //}

    }

}
