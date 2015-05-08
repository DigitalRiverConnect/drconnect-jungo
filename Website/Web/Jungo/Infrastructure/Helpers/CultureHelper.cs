using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Definitions;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers
{
    public static class CultureHelper
    {
        /// <summary>
        /// Parses Http Header UserLanguages into an array of locales and/or languages
        /// </summary>
        public static IEnumerable<string> GetLocalesFromUserLanguages(IEnumerable<string> names)
        {
            // handle null properly
            if (names == null) return Enumerable.Empty<string>();

            var locales = names.Select(n =>
            {
                var locale = n;
                int i = locale.IndexOf(';'); // may contain a quality specifier de-de;q=x
                if (i >= 0)
                    locale = locale.Substring(0, i);

                return locale;
            });
            return locales;
        }

        /// <summary>
        /// Returns the best matching start page for the given locale names
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="names">array</param>
        /// <returns></returns>
        public static LanguageRoot GetFirstMatchingLanguageRootOrDefault(ContentItem currentPage, IEnumerable<string> names)
        {
#if DEBUG_T
            Stopwatch timer = new Stopwatch();
            timer.Start();
#endif
            LanguageRoot translation = null;
            var translations = CmsFinder.FindTranslationsOf(currentPage).ToArray();
            if (translations.Length == 1)
                translation = translations.First(); // no options, why check
            else if (names != null)
            {
                var locales = GetLocalesFromUserLanguages(names).ToArray();

                // match full locales list
                translation = translations.FirstOrDefault(t => locales.Any(l => t.LanguageCode.StartsWith(l, StringComparison.InvariantCultureIgnoreCase))) 
                           ?? translations.FirstOrDefault(t => locales.Any(l => t.LanguageCode.StartsWith(l.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase)));
            }

            if (translation == null)
                translation = translations.FirstOrDefault(); // TODO use configured default, otherwise assume first by sort order           

            if (translation == null)
                throw new Exception("no translations found for " + currentPage);
#if DEBUG_T
            timer.Stop();
            locales = locales ?? GetLocalesFromUserLanguages(names).ToArray(); // can contain en-us, en, de-de etc.
            var codes = String.Join(", ", translations.Select(t => t.LanguageCode));
            Logger.InfoFormat("{3} ms LanguageRoot from locales {0} and translations {1} => {2}",  
                String.Join(", ", locales), codes, translation,  timer.Elapsed.TotalMilliseconds);
#endif
            return translation;
        }

        private static readonly string[] DefaultLangs = { "en-us" };
        /// <summary>
        /// Picks the translation best matching the browser-language or the first translation in the list
        /// typically called from LanguageIntersectionController
        /// compare ContentController.SetCulture for actual Culture setting.
        /// 
        /// Picks the first site and start page that matches the user's culture codes.
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="userLanguages"></param>
        /// <returns></returns>#
        public static ContentItem SelectLanguage(this ContentItem currentPage, IList<string> userLanguages)
        {
#if DEBUG_T
            Stopwatch timer = new Stopwatch();
            timer.Start();
#endif

            // TODO: If multiple sites have start pages that match of of the user's culture codes, is there some other way to fine-tune 
            // TODO: site selection instead of arbitrarily picking the first one?
            var start = Find.ClosestOf<IStartPage>(currentPage) ?? Find.StartPage;

            if (start is LanguageIntersection || start is StartPage)
            {
                if (userLanguages != null && userLanguages.Count > 0)
                    start = GetFirstMatchingLanguageRootOrDefault(currentPage, userLanguages);
                else
                    start = GetFirstMatchingLanguageRootOrDefault(currentPage, DefaultLangs);
            }

#if DEBUG_T
            timer.Stop();
            Logger.InfoFormat("{3} ms SelectLanguage for Page {0} and languages {1} => {2}", 
                currentPage, userLanguages, start, timer.Elapsed.TotalMilliseconds);
#endif            
            //if (start == null) Logger.Error("SelectLanguage failed: " + userLanguages);
            return start;
        }

        /// <summary>
        /// Create links to other translations
        /// </summary>
        /// <returns></returns>
        public static IHtmlString TranslationLinks()
        {
            var sb = new StringBuilder();

            // TODO could also check for translation of current item
            foreach (var locale in CmsFinder.FindTranslationsOf(CmsFinder.FindLanguageIntersection()).Where(loc => !string.IsNullOrEmpty(loc.LanguageCode)))
            {
                var anchor = new TagBuilder("a");
                anchor.MergeAttribute("href", locale.Url);
                anchor.MergeAttribute("hreflang", string.IsNullOrWhiteSpace(locale.LanguageCode) ? string.Empty : locale.LanguageCode.Substring(0, 2));
                // var ci = CultureInfo.CreateSpecificCulture(locale.LanguageCode);
                //anchor.InnerHtml = ci.DisplayName;
                anchor.InnerHtml = locale.LanguageTitle;

                sb.Append(anchor);
                sb.Append("</br>");
            }

            return new HtmlString(sb.ToString());
        }
    }
}