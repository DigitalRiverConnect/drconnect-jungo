using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models;
using N2;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pickers
{
    public class ContentPickerController : Controller
    {
        //
        // GET: /ContentPicker/

        [HttpGet]
        public ActionResult Index(string id, int startPageId)
        {
            // TODO: Need error handling?
            
            // ToDo: Research -- is there a way to .filter out anyitem that does not implment 'iPage'  ??
            var startPage = Find.Items.Where.ID.Eq(startPageId).Select().Single();

            //N2.Definitions.IPage 

            var viewModel = new ContentPickerViewModel
            {
                SearchCriteria = "",
                ControlId = id,
                StartPage = CreateContentItemViewModel(startPage)
            };

            return View(viewModel);         
        }

        private ContentItemViewModel CreateContentItemViewModel(ContentItem contentItem)
        {
            var vm = new ContentItemViewModel
                {
                    IsPage = contentItem.IsPage,
                    Name = contentItem.Name,
                    Title = contentItem.Title,
                    Url = contentItem.Url,
                    Children = contentItem.Children.Select(CreateContentItemViewModel).ToArray()
                };

            return vm;
        }
    }

}
