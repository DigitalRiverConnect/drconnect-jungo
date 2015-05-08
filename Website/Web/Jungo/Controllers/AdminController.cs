
#if !DEBUG
using DigitalRiver.CloudLink.Commerce.Web.Mvc;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Persistence;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    public class AdminController : Controller
    {
        //private readonly IAdministrationService _administrationService;
        private readonly IContentItemRepository _contentItemRepository;

        public AdminController(/*IAdministrationService administrationService,*/ IContentItemRepository contentItemRepository)
        {
            //_administrationService = administrationService;
            _contentItemRepository = contentItemRepository;
        }

#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult FlushCache()
        {
#if false
            _cacheService.FlushAll();
            _administrationService.FlushCache();
#endif
            return Content("Done! well, not really");
        }

        private const string ReplicationResultsHtml = @"
<html>
<head>
<style>
table, th, td {{ border: 1px solid black; }}
table {{ border-collapse:collapse; margin-left:5px;}}
th {{ background-color:#3c78b5; color:white; }}
th, td {{ padding:3px; }}
.numcol {{text-align:right}}
</style>
</head>
<body>
Advanced each publish date by one second on content items:<br>
<table>
<tr><th>Site</th><th>content items</th></tr>
{0}
</table><br>
This should force replication.
</body></html>";

        private const string ReplicationNoSiteHtml = @"
<html><body>
To force replication for a site, supply the site ID on the URL, such as .../ForceReplication/mssg<br/>
Choose site ID from among {{ {0} }} or ALL.
</body></html>";

        // URL: /Restricted/Admin/ForceReplication/siteId [ or siteId ALL for all sites ]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult ForceReplication(string id)
        {
#if false
            if (string.IsNullOrEmpty(id))
                return Content(
                    string.Format(ReplicationNoSiteHtml,
                        string.Join(",", CmsFinder.FindAll<StartPage>().Select(s => s.SiteID))),
                    "text/html; charset=UTF-8");

            var startPages = GetSites(id);
            if (startPages.Count == 0)
                return Content(id + " is not a valid site id");

            var results = new Dictionary<string, int>();
            foreach (var startPage in startPages)
            {
                var childCount = AdvancePubDateForSite(startPage);
                results.Add(startPage.SiteID, childCount);
            }

            _contentItemRepository.Flush();

            var resultTableRows = results.Aggregate(string.Empty,
                (current, result) =>
                    current +
                    string.Format("<tr><td>{0}</td><td class='numcol'>{1}</td></tr>", result.Key, result.Value));
            return Content(string.Format(ReplicationResultsHtml, resultTableRows), "text/html; charset=UTF-8");
#endif
            return Content("Done! well, not really");
        }

        private static IList<StartPage> GetSites(string siteId)
        {
            var startPages = new List<StartPage>();
            if (string.Compare("all", siteId, StringComparison.OrdinalIgnoreCase) == 0)
                startPages.AddRange(CmsFinder.FindAll<StartPage>());
            else
            {
                var startPage = CmsFinder.FindSitePageFromSiteId(siteId);
                if (startPage != null)
                    startPages.Add(startPage);
            }
            return startPages;
        }

        private int AdvancePubDateForSite(ContentItem startPage)
        {
            var childs = CmsFinder.FindAllDescendentsOf<ContentItem>(startPage);
            childs.Add(startPage);
            var childCount = 0;
            foreach (var child in childs.Where(child => child.IsPublished()))
            {
                child.Published = child.Published.HasValue ? child.Published.Value.AddSeconds(1.0) : DateTime.Now;
                _contentItemRepository.SaveOrUpdate(child);
                ++childCount;
            }
            return childCount;
        }
    }
}
