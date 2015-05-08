using System.Web.Mvc;
using N2.Models;
using N2.Web;
using N2.Web.Mvc;

namespace N2.Controllers
{
    [Controls(typeof(PartDefinitionPage))]
    public class PartDefinitionPageController : ContentController<PartDefinitionPage>
    {
        public override ActionResult Index()
        {
            return new EmptyResult();
        }
    }
}