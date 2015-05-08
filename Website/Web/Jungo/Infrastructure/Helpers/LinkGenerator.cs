using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using Jungo.Infrastructure.Cache;
using Jungo.Infrastructure.Logger;
using N2;
using N2.Engine;
using N2.Persistence;
using N2.Plugin;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers
{
    [Service(typeof(ILinkGenerator))]
    public class LinkGenerator : ILinkGenerator, IFlushable
    {
        private readonly ICache<string> _urlCache;
        private readonly ICache<object> _itemCache;
        private readonly IRequestLogger _logger;

        public LinkGenerator(IPersister persister, ConnectionMonitor connection, CacheWrapper cacheWrapper, IRequestLogger logger)
        {
            _logger = logger;

            // DON'T USE DISTRIBUTED CACHE FOR N2 internal data 
            // _urlCache = cacheService.GetCache<string>(new CacheConfig("LinkGenerator_urlCache", 1800));
            _urlCache = new HttpRuntimeCacheWrapper<string>("LinkGenerator_urlCache", cacheWrapper);
            _itemCache = new HttpRuntimeCacheWrapper<object>("LinkGenerator_urlCache_item", cacheWrapper);

            // hook up to persister events
            connection.Online += delegate
            {
                persister.ItemSaved += persister_ItemSaved;
                persister.ItemMoving += persister_ItemMoving;
                persister.ItemMoved += persister_ItemMoved;
                persister.ItemCopied += persister_ItemCopied;
                persister.ItemDeleted += persister_ItemDeleted;
                persister.FlushCache += persister_ItemInvalidated;
            };
            connection.Offline += delegate
            {
                persister.ItemSaved -= persister_ItemSaved;
                persister.ItemMoving -= persister_ItemMoving;
                persister.ItemMoved -= persister_ItemMoved;
                persister.ItemCopied -= persister_ItemCopied;
                persister.ItemDeleted -= persister_ItemDeleted;
                persister.FlushCache -= persister_ItemInvalidated;
            };
        }

        private void persister_ItemInvalidated(object sender, EventArgs e)
        {
            Flush();
        }

        #region IFlushable Members

        public void Flush()
        {
            _logger.Debug("LinkGenerator.FlushCache() " + _urlCache.GetType().FullName);
            _urlCache.Flush();
            _itemCache.Flush();
        }

        #endregion

        #region Change Events (will Flush Cache)

        void persister_ItemDeleted(object sender, ItemEventArgs e)
        {
            ItemDeleted(e.AffectedItem);
        }

        void persister_ItemCopied(object sender, DestinationEventArgs e)
        {
            ItemChanged(e.AffectedItem);
        }

        void persister_ItemMoving(object sender, CancellableDestinationEventArgs e)
        {
            var originalAction = e.FinalAction;
            e.FinalAction = (from, to) =>
            {
                var result = originalAction(from, to);
                ItemDeleted(from);
                return result;
            };
        }

        void persister_ItemMoved(object sender, DestinationEventArgs e)
        {
            ItemChanged(e.AffectedItem);
        }

        void persister_ItemSaved(object sender, ItemEventArgs e)
        {
            ItemChanged(e.AffectedItem);
        }

        private void ItemChanged(ContentItem item)
        {
            if (item.ID <= 0)
                return;

            if (item is PageModelBase)
                Flush();
        }

        private void ItemDeleted(ContentItem item)
        {
            if (item.ID <= 0)
                return;

            if (item is PageModelBase)
                Flush();
        }

        #endregion

        #region ILinkGenerator Members

        private void AddCache(string cacheKey, string value)
        {
#if DEBUG_T
            Logger.InfoFormat("Link[{0}] = {1}", cacheKey, value);
#endif
            _urlCache.Add(cacheKey, value);
        }

        private void AddCache(string cacheKey, string value, ContentItem contentItem)
        {
            AddCache(cacheKey, value);
            _itemCache.Add(cacheKey, contentItem);
        }

        public string GenerateCategoryLink(long? categoryId, bool? forceListPage, out object matchingCategoryPage)
        {
            //~~  a cheap and nasty chaos monkey
            //if (DateTime.Now.Minute % 2 == 1)
            //    throw new Exception("chaos happened " + categoryId);
            //~~

            string ret;
            var cacheKey = string.Format("{0} {1} {2} {3} {4}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "CategoryLink", categoryId, forceListPage);
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                var catPages = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<CatalogPage>();

                if (catPages.All(cps => cps.CategoryID.Length != 0))
                {
                    // find a category page from the root level that has no category ID
                    var defaultCatPage = CmsFinder.FindAllNonSiteDescendentsOfRoot<CatalogPage>()
                             .LastOrDefault(cps => cps.CategoryID.Length == 0);

                    if (defaultCatPage != null)
                        catPages.Add(defaultCatPage);
                }

                CatalogPage cp;

                if (forceListPage.HasValue && forceListPage.Value)
                    cp = catPages.LastOrDefault(catalogPage => catalogPage.CategoryID.Length == 0);
                else
                    cp = catPages.FirstOrDefault(catalogPage => catalogPage.CategoryID.Equals(categoryId.ToString(), StringComparison.InvariantCultureIgnoreCase)) ??
                             catPages.LastOrDefault(catalogPage => catalogPage.CategoryID.Length == 0);

                if (cp == null)
                    ret = null;
                else
                {
                    // If the category page came from the root, append it's name and parameters to the current page's url
                    var url = new Url(GetNormalizedPageUrl(cp));

                    if (string.IsNullOrEmpty(cp.CategoryID) && categoryId != null)
                        url = url.Append(categoryId.Value.ToString(CultureInfo.InvariantCulture));

                    if (forceListPage.HasValue && forceListPage.Value && string.IsNullOrEmpty(cp.CategoryID))
                        url = url.AppendQuery("list=true");

                    ret = url.ToString();
                }

                AddCache(cacheKey, ret, cp);
            }

            _itemCache.TryGet(cacheKey, out matchingCategoryPage);

            return ret;
        }

        public string GenerateCategoryLink(long? categoryId = null, bool? forceListPage = null)
        {
            object cp;
            return GenerateCategoryLink(categoryId, forceListPage, out cp);
        }

        public string GenerateProductLink(long? productId, out object matchingProductPage)
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2} {3}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "ProductLink", productId);
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                try
                {
                    var prodPages = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ProductPage>();
                    if (prodPages.All(pp => !string.IsNullOrEmpty(pp.ProductID)))
                    {
                        // find a product page from the root level that has no product ID
                        var page = CmsFinder.FindAllNonSiteDescendentsOfRoot<ProductPage>()
                                            .LastOrDefault(pp => string.IsNullOrEmpty(pp.ProductID));

                        if (page != null)
                            prodPages.Add(page);
                    }

                    var targetProductPage =
                        prodPages.FirstOrDefault(
                            pp => pp.ProductID.Split().Any(pid => pid.Equals(productId.ToString(), StringComparison.InvariantCultureIgnoreCase))) ??
                        prodPages.LastOrDefault(pp => pp.ProductID.Length == 0);

                    if (targetProductPage == null)
                        ret = null;
                    else
                    {
                        var url = new Url(GetNormalizedPageUrl(targetProductPage));

                        if ((string.IsNullOrEmpty(targetProductPage.ProductID) || targetProductPage.ProductID.Split().Length > 1)
                            && productId != null)
                            url = url.Append(productId.ToString());

                        ret = url.ToString();
                    }

                    AddCache(cacheKey, ret, targetProductPage);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to create product link for product {0}", productId);
                }
            }

            _itemCache.TryGet(cacheKey, out matchingProductPage);

            return ret;
        }

        public string GenerateProductLink(long? productId = null)
        {
            object pp;
            return GenerateProductLink(productId, out pp);
        }

        public string GenerateShoppingCartLink()
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2}", WebSession.Current.SiteId, WebSession.Current.LanguageCode, "CartLink");
            
            if (_urlCache.TryGet(cacheKey, out ret)) return ret;

            var cartPage = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ShoppingCartPage>().FirstOrDefault();
            ret = GetNormalizedPageUrl(cartPage);
            AddCache(cacheKey, ret);

            return ret;
        }

        public string GenerateCheckoutLink()
        {
            return null;
            //string ret;
            //var cacheKey = string.Format("{0} {1} {2}", WebSession.Current.SiteId, WebSession.Current.LanguageCode, "CheckoutLink");
            //if (_urlCache.TryGet(cacheKey, out ret)) return ret;

            //var ckoPage = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<CheckoutPage>().FirstOrDefault();

            //ret = GetNormalizedPageUrl(ckoPage);
            //AddCache(cacheKey, ret);

            //return ret;
        }

        private string GetNormalizedPageUrl(ContentItem pageToNormalize)
        {
            if (pageToNormalize == null) return null;

            if (pageToNormalize is LanguageRoot
                || pageToNormalize is StartPage
                || pageToNormalize is LanguageIntersection)
                return GetRootPath(pageToNormalize);

            string baseUrl = null;

            var context = Context.Current.RequestContext;
            // should use DI - NOT DependencyResolver.Current.Get<IWebContext>();
            var currentPage = context.CurrentPage;

            if (currentPage != null)
            {
                // Attempt to take the baseUrl from the current page.
                var languageRoot = CmsFinder.FindLanguageRootOf(currentPage);
                if (languageRoot != null)
                    baseUrl = languageRoot.Url;
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                // Attempt to take the baseUrl from the pageToNormalize
                var languageRoot = CmsFinder.FindLanguageRootOf(pageToNormalize);
                if (languageRoot != null)
                    baseUrl = languageRoot.Url;
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                // Attempt to take the baseUrl from the URL
                var pageUrlFragments = context.Url.ApplicationRelativePath.Split('/');

                // verify that the first two fragments are the siteId and language
                if (pageUrlFragments.Length >= 2)
                {
                    var fragment1 = pageUrlFragments[0];
                    var fragment2 = pageUrlFragments[1];

                    var startPage =
                        CmsFinder.FindLanguageIntersection()
                                 .GetChildren<StartPage>()
                                 .FirstOrDefault(
                                     sp => sp.SiteID.Equals(fragment1, StringComparison.InvariantCultureIgnoreCase));

                    if (startPage != null)
                    {
                        var languageRoot =
                            startPage.GetChildren<LanguageRoot>()
                                     .FirstOrDefault(
                                         lr =>
                                         lr.LanguageCode.Equals(fragment2, StringComparison.InvariantCultureIgnoreCase));

                        if (languageRoot != null)
                            baseUrl = string.Concat(pageUrlFragments[0], "/", pageUrlFragments[1]);
                    }
                }
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                // Try to use websession.
                if (WebSession.IsInitialized && !string.IsNullOrEmpty(WebSession.Current.SiteId) && !string.IsNullOrEmpty(WebSession.Current.CultureCode))
                    baseUrl = string.Concat(WebSession.Current.SiteId, "/", WebSession.Current.CultureCode);
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                // Try to get the baseUrl based on the user's language.
                var language = pageToNormalize.SelectLanguage(HttpContext.Current.Request.UserLanguages);
                baseUrl = language.Url;
            }

            if (string.IsNullOrEmpty(baseUrl)) // We are unable to form a valid base Url
                return null;

            // build page URL from the page and it's ancestors.
            string pageUrl = GetRelativePathForItem(pageToNormalize);

            var relativeUrl = Url.Combine(baseUrl.ToLowerInvariant(), pageUrl);
            relativeUrl = Url.Combine(context.HttpContext.Request.ApplicationPath, relativeUrl);

            return Url.Combine(context.Url.HostUrl, relativeUrl);
        }

        public string GenerateSearchActionLink()
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2}", WebSession.Current.SiteId, WebSession.Current.LanguageCode, "SearchActionLink");
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                var page = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<SearchPage>().LastOrDefault();

                if (page == null)
                    ret = null;
                else
                {
                    // If the category page came from the root, append it's name and parameters to the current page's url
                    var url = new Url(GetNormalizedPageUrl(page));
                    ret = url.ToString();
                }

                AddCache(cacheKey, ret);
            }
            return ret;
        }

        public string GenerateStoreLink()
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "StoreLink");
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                var page = (ContentPage)CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<StoreHomePage>().FirstOrDefault();

                if (page == null) // stop-gap - should be getting rid of plain ContentPage with a name of "store"
                    page = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ContentPage>(p => p.Name.Equals("store", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                ret = page == null ? null : GetNormalizedPageUrl(page);

                AddCache(cacheKey, ret);
            }

            return ret;
        }

        public string GenerateSearchLink()
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "SearchLink");
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                var page = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<SearchPage>().FirstOrDefault() ??
                           CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<SearchPage>(p => p.Name.Equals("search", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                ret = page == null ? null : GetNormalizedPageUrl(page);

                AddCache(cacheKey, ret);
            }

            return ret;
        }

        public string GenerateFAQLink()
        {
            string ret;

            var cacheKey = string.Format("{0} {1} {2}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "FaqLink");
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                var page = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<FaqPage>().FirstOrDefault();
                ret = page == null ? null : new Url(GetNormalizedPageUrl(page)).ToString();
                AddCache(cacheKey, ret);
            }

            return ret;
        }

        //public string GenerateNotFoundLink()
        //{
        //    string ret;

        //    var cacheKey = CacheKeyUtils.Get("NotFoundLink");
        //    if (!_urlCache.TryGet(cacheKey, out ret))
        //    {
        //        var notFoundPages = FindUtils.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<NotFoundPage>();
        //        ret = notFoundPages.Any() ? GetNormalizedPageUrl(notFoundPages.First()) : null;
        //        AddCache(cacheKey, ret);
        //    }

        //    return ret;
        //}

        //public string GenerateServerErrorLink()
        //{
        //    string ret;
        //    var cacheKey = CacheKeyUtils.Get(WebSession.Current.LanguageCode, "ServerErrorLink");
        //    if (!_urlCache.TryGet(cacheKey, out ret))
        //    {
        //        var svrerrPages = FindUtils.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ServerErrorPage>();

        //        ret = svrerrPages.Any() ? GetNormalizedPageUrl(svrerrPages.First()) : null;
        //        AddCache(cacheKey, ret);
        //    }

        //    return ret;
        //}

        public string GenerateLinkForNamedContentItem(string name)
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2}",
                WebSession.Current.LanguageCode, "NamedContentItem", name);
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                // TODO: Should "store" be configurable?
                var page = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<PageModelBase>(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                ret = page == null ? null : GetNormalizedPageUrl(page);

                AddCache(cacheKey, ret);
            }

            return ret;
        }

        #endregion

        #region Private Methods

        private string GetRootPath(ContentItem rootItem)
        {
            var stack = new Stack<ContentItem>();
            bool isDone = rootItem is LanguageIntersection;
            while (!isDone)
            {
                stack.Push(rootItem);
                rootItem = rootItem.Parent;

                isDone = rootItem is LanguageIntersection;
            }

            return Url.Combine(HttpContext.Current.Request.ApplicationPath, BuildUrlFromStack(stack));
        }

        private string BuildUrlFromStack(Stack<ContentItem> stack)
        {
            if (stack == null || stack.Count == 0)
                return null;

            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                var stackItem = stack.Pop();
                sb.Append(stackItem.Name);
                sb.Append("/");
            }

            if (sb.Length > 0)
                sb.Length--;

            return sb.ToString();
        }

        private string GetRelativePathForItem(ContentItem item)
        {
            var stack = new Stack<ContentItem>();
            var currentItem = item;
            bool isDone = currentItem is LanguageRoot || currentItem is StartPage || currentItem is LanguageIntersection || currentItem == null;

            while (!isDone)
            {
                stack.Push(currentItem);
                currentItem = currentItem.Parent;

                isDone = currentItem is LanguageRoot || currentItem is StartPage || currentItem is LanguageIntersection || currentItem == null;
            }

            return BuildUrlFromStack(stack);
        }

        #endregion


        public string GenerateInterstitialLink()
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2} {3}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "InterstitialLink", "generic");
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                try
                {
                    var interstitialPage = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ShoppingCartInterstitialPage>().FirstOrDefault(ip => string.IsNullOrEmpty(ip.ProductID));

                    if (interstitialPage == null)
                        ret = null;
                    else
                    {
                        var url = new Url(GetNormalizedPageUrl(interstitialPage));
                        ret = url.ToString();
                    }

                    AddCache(cacheKey, ret);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to create shopping cart POP interstitial link");
                }
            }

            return ret;
        }

        public string GenerateInterstitialLink(long? productId)
        {
            string ret;
            var cacheKey = string.Format("{0} {1} {2} {3}",
                WebSession.Current.SiteId, WebSession.Current.LanguageCode, "InterstitialLink", productId);
            if (!_urlCache.TryGet(cacheKey, out ret))
            {
                try
                {
                    var interstitialPages = CmsFinder.FindDescendentsOrFallbackOfCurrentPageLanguageRoot<ShoppingCartInterstitialPage>();
                    var targetInterstitialPage =
                        interstitialPages.FirstOrDefault(
                            ip => ip.ProductID.Split().Any(pid => pid.Equals(productId.ToString(), StringComparison.InvariantCultureIgnoreCase)));

                    if (targetInterstitialPage == null)
                        ret = null;
                    else
                    {
                        var url = new Url(GetNormalizedPageUrl(targetInterstitialPage));

                        if ((string.IsNullOrEmpty(targetInterstitialPage.ProductID) || targetInterstitialPage.ProductID.Split().Length > 1)
                            && productId != null)
                            url = url.Append(productId.ToString());

                        ret = url.ToString();
                    }

                    AddCache(cacheKey, ret);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to create shopping cart interstitial link for product {0}", productId);
                }
            }

            return ret;
        }
    }
}
