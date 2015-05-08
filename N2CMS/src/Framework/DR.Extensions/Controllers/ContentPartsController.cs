using System.Web.Mvc;
using N2.Models;
using N2.Web;
using N2.Web.Mvc;

namespace N2.Controllers
{
	[Controls(typeof(ContentPart))]
	public class ContentPartsController : ContentController<ContentPart>
	{
		public override ActionResult Index()
		{
			var tmpl = CurrentItem.TemplateKey;
			int id;
			if (int.TryParse(tmpl, out id))
			{
				// TODO - this won't work with drafts!
				// content item based view
				if (id == 0) id = CurrentItem.ID;
				if (id > 0)
					return PartialView(id.ToString(), CurrentItem);

				tmpl = null;
			}

			if (string.IsNullOrEmpty(tmpl))
				return Content(string.Format("[ContentPart {0}]", CurrentItem.Title));

			return PartialView("PartTemplates/" + tmpl, CurrentItem);
		}
	}
}