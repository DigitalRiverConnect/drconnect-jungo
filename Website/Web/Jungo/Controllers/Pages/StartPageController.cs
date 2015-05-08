using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2;
using N2.Definitions;
using N2.Engine.Globalization;
using N2.Persistence.Search;
using N2.Web;
using N2.Web.Mvc;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages
{
    [Controls(typeof(StartPage))]
    public class StartPageController : ContentControllerBase<StartPage> // ContentController<StartPage>
    {
        public StartPageController(IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            
        }

        public override ActionResult Index()
        {
            AssertProductsLoaded();
            // detect language from browser settings
            if (!IsManaging)
            {
                var language = CurrentItem.SelectLanguage(Request.UserLanguages);
                if (language != null && language != CurrentItem)
                {
                    return RedirectPermanent(language.Url);
                }

                if (CurrentItem.RedirectUrl != CurrentItem.Url)
                    return RedirectPermanent(CurrentItem.RedirectUrl);
            }

            SetPageTitleSimple();
            return View();
        }


		public override ActionResult NotFound()
		{
			var closestMatch = ContentHelper.Traverse.Path(Request.AppRelativeCurrentExecutionFilePath.Trim('~', '/')).StopItem;
			
			var startPage = ContentHelper.Traverse.ClosestStartPage(closestMatch);
			var urlText = Request.AppRelativeCurrentExecutionFilePath.Trim('~', '/').Replace('/', ' ');
			var similarPages = GetSearchResults(startPage, urlText, 10).ToList();

			ControllerContext.RouteData.ApplyCurrentPath(new PathData(new ContentPage { Parent = startPage }));
			Response.TrySkipIisCustomErrors = true;
			Response.Status = "404 Not Found";

			return View(similarPages);
		}


		private IEnumerable<ContentItem> GetSearchResults(ContentItem root, string text, int take)
		{
            //TODO: commented out original, added ".Take(take)" below; is this OK???
            var query = Query.For(text).Below(root).Take(take).ReadableBy(User, Roles.GetRolesForUser).Except(Query.For(typeof(ISystemNode)));
			var hits = Engine.Resolve<IContentSearcher>().Search(query).Hits.Select(h => h.Content);
			return hits;
		}

		[ContentOutputCache]
		public ActionResult Translations(int id)
		{
			var sb = new StringBuilder();

			var item = Engine.Persister.Get(id);
			var lg = Engine.Resolve<LanguageGatewaySelector>().GetLanguageGateway(item);
			var translations = lg.FindTranslations(item);
			foreach (var language in translations)
				sb.Append("<li>").Append(Link.To(language).Text(lg.GetLanguage(language).LanguageTitle)).Append("</li>");

			if (sb.Length == 0)
				return Content("<ul><li>This page is not translated</li></ul>");

			return Content("<ul>" + sb + "</ul>");
		}
	}
}
