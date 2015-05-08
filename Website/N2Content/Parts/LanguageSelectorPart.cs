using System.Collections.Generic;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition(Title = "Language Selector", SortOrder = 120, IconUrl = "~/Content/img/icons/globe.png")]
    public class LanguageSelectorPart : PartModelBase
    {
        public IList<LanguageRoot> Languages { get; set; }
    }
}
