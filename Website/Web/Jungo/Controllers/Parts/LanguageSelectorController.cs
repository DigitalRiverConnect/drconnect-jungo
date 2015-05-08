using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(LanguageSelectorPart))]
    public class LanguageSelectorController : PartControllerBase<LanguageSelectorPart>
    {
        public LanguageSelectorController(IRequestLogger logger) : base(logger)
        {
        }

        public override ActionResult Index()
        {
            var vm = new LanguageSelectorPartViewModel();

            if (WebSession.IsInitialized)
            {
                var siteRoot = CmsFinder.FindSitePageFromSiteId(WebSession.Current.SiteId);

                if (siteRoot != null)
                {
                    vm.Languages = siteRoot.GetChildren<LanguageRoot>().Select(l => new Language
                        {
                            FlagUrl = l.FlagUrl,
                            LanguageCode = l.LanguageCode,
                            LanguageTitle = l.LanguageTitle
                        }).ToList();
                }

                return PartialView(vm);
            }

            vm.Languages = new List<Language>();

            return PartialView(vm);
        }
    }
}
