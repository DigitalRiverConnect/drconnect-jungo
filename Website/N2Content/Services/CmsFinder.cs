//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2;
using N2.Collections;
using N2.Definitions;
using N2.Engine;
using N2.Persistence;
using N2.Persistence.Finder;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services
{
    public interface IContentFinder
    {
        IEnumerable<T> FindAll<T>(AccessFilter accessFilter = null, PublishedFilter publishedFilter = null) where T : ContentItem;

        IEnumerable<T> FindAllDescendentsOf<T>(ContentItem ancestor, AccessFilter accessFilter = null,
                                               PublishedFilter publishedFilter = null) where T : ContentItem;
    }

    [Service(typeof(IContentFinder))]
    public class ContentFinder : IContentFinder
    {
        private readonly IItemFinder _itemFinder; // TODO maybe create a new interface instead of extending deprecated one

        public ContentFinder(IItemFinder itemFinder)
        {
            _itemFinder = itemFinder;
        }

        public IEnumerable<T> FindAll<T>(AccessFilter accessFilter, PublishedFilter publishedFilter) where T : ContentItem
        {
            var filter = new AllFilter(accessFilter ?? new AccessFilter(), publishedFilter ?? new PublishedFilter());
            return _itemFinder.AllOfType<T>().Where(filter.Match);
        }

        public IEnumerable<T> FindAllDescendentsOf<T>(ContentItem ancestor, AccessFilter accessFilter = null, PublishedFilter publishedFilter = null) where T : ContentItem
        {
            if (accessFilter == null && publishedFilter == null)
                return _itemFinder.AllOfType<T>().Where(i => i.AncestralTrail.StartsWith(ancestor.GetTrail()));
                
            var filter = new AllFilter(accessFilter ?? new AccessFilter(), publishedFilter ?? new PublishedFilter());
            return _itemFinder.AllOfType<T>().Where(i => i.AncestralTrail.StartsWith(ancestor.GetTrail())).Where(filter.Match);
        }
    }

    /// <summary>
    /// Methods for finding nodes in a content tree.
    /// Nomenclature:
    /// ROOT
    /// + LanguageIntersection (= Company - referred to here also as "Root")
    ///   + various pages not of type StartPage (referred to here as "NonSiteDescendents")
    ///   + StartPage (= Country/Site, referred to here also as "Site")
    ///     + LanguageRoot (=Locale)
    ///       + various pages (not "NonSiteDescendents")
    ///         + various pages ( " )
    ///     + page x (default page x)
    ///       + LanguageRoot (one of "TranslationsOf" page x)
    ///         + page y
    ///       + LanguageRoot (another "TranslationsOf" page x)
    ///         + page z
    ///   + StartPage (= Country/Site)
    ///   ...
    /// </summary>
    public class CmsFinder
    {
        /// <summary>
        /// for breaking dependency on N2 engine during tests
        /// </summary>
        /// <param name="resolver"></param>
        public static void SetSessionInfoResolver(ISessionInfoResolver resolver)
        {
            _sessionInfoResolver = resolver;
        }
        private static ISessionInfoResolver _sessionInfoResolver;

        private static ISessionInfoResolver SessionInfoResolver
        {
            get { return _sessionInfoResolver ?? (_sessionInfoResolver = Context.Current.Container.Resolve<ISessionInfoResolver>()); }
        }

        public static LanguageRoot FindLanguageRootOf(ContentItem item)
        {
            return (LanguageRoot)GenericFind<ContentItem, ContentItem>.ClosestOf<LanguageRoot>(item);
        }

        public static ContentItem FindStartPageOf(ContentItem item)
        {
            return GenericFind<ContentItem, ContentItem>.ClosestOf<IStartPage>(item);
        }

        /// <summary>
        /// Get Translations of the current page
        /// </summary>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public static IEnumerable<LanguageRoot> FindTranslationsOf(ContentItem currentPage)
        {
            // slow on SQL, fast in Memory return GenericFind<ContentItem, ContentItem>.EnumerateChildren(currentPage).OfType<LanguageRoot>();
            //return Finder().FindAll<LanguageRoot>().Where(i => i.AncestralTrail.StartsWith(currentPage.GetTrail()));
            return FindAllDescendentsOf<LanguageRoot>(currentPage);
        }

        #region Finder Wrapper

        // internal implementation should use IContentFinder directly, where possible
        public static IContentFinder TheFinder = null; // for breaking dependency on N2 engine during tests
        public static IContentFinder Finder()
        {
            return TheFinder ?? Context.Current.Container.Resolve<IContentFinder>(); // TODO
        }

        public static IList<T> FindAll<T>() where T : ContentItem
        {
            return Finder().FindAll<T>().ToList();
        }

        public static LanguageIntersection FindLanguageIntersection()
        {
            return Finder().FindAll<LanguageIntersection>().FirstOrDefault();
        }

        public static IList<T> FindAllDescendentsOf<T>(ContentItem ancestor) where T : ContentItem
        {
            return Finder().FindAllDescendentsOf<T>(ancestor).ToList();
        }

        #endregion

        #region Private

        private static IEnumerable<T> InternalFindAllNonSiteDescendentsOfRoot<T>(AccessFilter accessFilter,
                                                                                 PublishedFilter publishedFilter, Func<T, bool> predicate = null)
            where T : ContentItem
        {
            var root = FindLanguageIntersection();
            var prefixes = Finder().FindAllDescendentsOf<StartPage>(root).Select(i => i.GetTrail()).ToList();
            var result = Finder().FindAllDescendentsOf<T>(root, accessFilter, publishedFilter);
            if (predicate != null)
                result = result.Where(predicate);

            // Exclude any items that are children of start pages.
            return result.Where(r => !prefixes.Any(pref => r.AncestralTrail.StartsWith(pref)));
        }

        public static IList<T> FindAllNonSiteDescendentsOfRoot<T>(Func<T, bool> predicate = null) where T : ContentItem
        {
            return InternalFindAllNonSiteDescendentsOfRoot(null, null, predicate).ToList();
        }

        private static IEnumerable<T> InternalFindAllNonLanguageRootDescendentsOfStartPage<T>(ContentItem currentPage)
            where T : ContentItem
        {
            var start = FindStartPageOf(currentPage);
            var prefixes = Finder().FindAllDescendentsOf<LanguageRoot>(start).Select(i => i.GetTrail()).ToList();
            var result = Finder().FindAllDescendentsOf<T>(start);

            // Exclude any items that are children of language roots.
            return result.Where(r => !prefixes.Any(pref => r.AncestralTrail.StartsWith(pref)));
        }

        #endregion

        public static IList<T> FindDescendentsOrFallbackOfCurrentPageLanguageRoot<T>(ContentItem currentPage, Func<T, bool> predicate = null) where T : ContentItem
        {
            IList<T> result = null;
            LanguageRoot languageRoot;
#if DEBUG_T
            Stopwatch timer = new Stopwatch();
            timer.Start();
#endif

            if (currentPage != null)
            {
                IEnumerable<T> pages = null;

                languageRoot = FindLanguageRootOf(currentPage);
                if (languageRoot != null)
                {
                    pages = Finder().FindAllDescendentsOf<T>(languageRoot);
                    if (predicate != null && pages != null)
                        pages = pages.Where(predicate).ToList();
                }

                if (pages == null || !pages.Any())
                {
                    // No pages found, so find non-language root descendents of nearest Start Page
                    pages = InternalFindAllNonLanguageRootDescendentsOfStartPage<T>(currentPage);
                    if (predicate != null)
                        pages = pages.Where(predicate).ToList();
                }

                if (!pages.Any())
                {
                    // No pages found, so find non start-page descendents of site root.
                    pages = InternalFindAllNonSiteDescendentsOfRoot<T>(null, null);
                    if (predicate != null)
                        pages = pages.Where(predicate).ToList();
                }

                result = pages.ToList();
            }

            if (result == null) // this really resolves to "if (currentPage == null)"
            {
                // no current page (ajax call?)
                // set current page to current language of current site.
                // TODO: BUG?? the following logic is quite different from the logic above for when there is a current page
                //   the code below requires there to be BOTH a start page AND a language root
                //   in order to see if there are any pages descended from either languageroot or start or root
                //   but the code above doesn't have this requirement
                var startPage = FindLanguageIntersection().GetChildren<StartPage>()
                                .FirstOrDefault(sp => sp.SiteID.Equals(SessionInfoResolver.SiteId, StringComparison.InvariantCultureIgnoreCase));

                if (startPage != null)
                {
                    languageRoot = startPage.GetChildren<LanguageRoot>()
                                       .FirstOrDefault(lr => lr.LanguageCode.Equals(SessionInfoResolver.CultureCode, StringComparison.InvariantCultureIgnoreCase));

                    if (languageRoot != null)
                    {
                        IList<T> pages = FindAllDescendentsOf<T>(languageRoot).ToList();
                        if (predicate != null)
                            pages = pages.Where(predicate).ToList();

                        if (pages.Count == 0)
                        {
                            pages = FindAllDescendentsOf<T>(startPage).ToList();
                            if (predicate != null)
                                pages = pages.Where(predicate).ToList();
                        }

                        if (pages.Count == 0)
                        {
                            pages = FindAllNonSiteDescendentsOfRoot<T>();
                            if (predicate != null)
                                pages = pages.Where(predicate).ToList();
                        }

                        result = pages;
                    }
                }
            }
#if DEBUG_T
            timer.Stop();
            var res = result == null ? "null" : String.Join(", ", result.Select(i => i.GetTrail()));
            Logger.InfoFormat("FindDescendentsOrFallbackOf<{0}>({1}) root {2} => {3} - {4} ms ", typeof(T).Name, currentPage, languageRoot, res, (int)timer.Elapsed.TotalMilliseconds);
#endif
            return result ?? new List<T>();
        }
        
        public static IList<T> FindDescendentsOrFallbackOfCurrentPageLanguageRoot<T>(Func<T, bool> predicate = null) where T : ContentItem
        {
            var webContext = Context.Current.RequestContext;
            return FindDescendentsOrFallbackOfCurrentPageLanguageRoot(webContext.CurrentPage, predicate);
        }

        public static StartPage FindSitePageFromSiteId(string siteId)
        {
            return FindLanguageIntersection()
                .GetChildren<StartPage>()
                .FirstOrDefault(sp => sp.SiteID.Equals(siteId, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}