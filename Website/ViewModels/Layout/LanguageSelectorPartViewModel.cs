using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class Language
    {
        public string FlagUrl { get; set; }

        /// <summary>The friendly name of the language.</summary>
        public string LanguageTitle { get; set; }

        /// <summary>The identifier of the language.</summary>
        public string LanguageCode { get; set; }
    }

    public class LanguageSelectorPartViewModel
    {
        public IList<Language> Languages { get; set; }
    }
}
