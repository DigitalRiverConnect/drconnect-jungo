//#define SIMPLE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Extensions;
using Jungo.Models.ShopperApi.Common;
using N2;
using N2.Definitions;
using N2.Engine.Globalization;
using N2.Models;
using N2.Web;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    // see http://en.wikipedia.org/wiki/Sitemaps
    public class SitemapController : Controller
    {
        private readonly ICatalogApi _catalogApi;
        private readonly ILinkGenerator _linkGenerator;
        private readonly ICategoryViewModelBuilder _catViewModelBuilder;
        private readonly List<SiteMapEntry> _entries = new List<SiteMapEntry>();
        private readonly HashSet<string> _seen = new HashSet<string>();
        private readonly HashSet<int> _seenChild = new HashSet<int>();
        private readonly SiteConfig _siteConfig;

        public SitemapController(ICatalogApi catalogApi, ICategoryViewModelBuilder catViewModelBuilder, ILinkGenerator linkGenerator)
        {
            _catalogApi = catalogApi;
            _linkGenerator = linkGenerator;
            _catViewModelBuilder = catViewModelBuilder;
            _siteConfig = ConfigLoader.Get<SiteConfig>();
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            _seen.Clear();
            _seenChild.Clear();
            _entries.Clear();

            var url = new Url(Request.Url).RemoveTrailingSegment().RemoveExtension();
            if (WebSession.Current.SiteId != null)
            {
                // add categories for this site
                var categories = await _catViewModelBuilder.GetCategoriesAsync(null, levels: 99).ConfigureAwait(false);
                AddCategoriesRecursive(categories);
            }
            AddContentPages(url);

            // sort sitemap by URL
            _entries.Sort(Comparer<SiteMapEntry>.Create((a, b) => string.Compare(a.Url, b.Url, StringComparison.Ordinal)));

            return new SiteMapXmlResult(_entries);
        }

        private void Add(SiteMapEntry se)
        {
            if (_seen.Contains(se.Url)) return;

            var pos = se.Url.IndexOf(WebSession.Current.SiteId, StringComparison.InvariantCultureIgnoreCase);
            if (pos != -1)
            {
                var cultureCodeSegment = se.Url.Substring(pos + 5);
                pos = cultureCodeSegment.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                if (pos != -1)
                    cultureCodeSegment = cultureCodeSegment.Substring(0, pos);
                SiteInfo siteInfo;
                if (!cultureCodeSegment.Equals(WebSession.Current.CultureCode, StringComparison.InvariantCultureIgnoreCase) &&
                    _siteConfig.TryGetSiteInfo(WebSession.Current.SiteId, cultureCodeSegment, out siteInfo))
                {
                    return; // because it is a url that contains a different culture code
                }
            }

            _entries.Add(se);
            _seen.Add(se.Url);
        }

        private void AddCategoriesRecursive(CategoryViewModel cvm)
        {
            if (_seen.Contains(cvm.CategoryId.ToString(CultureInfo.InvariantCulture)))
                return;

            _seen.Add(cvm.CategoryId.ToString(CultureInfo.InvariantCulture));

            var addCat = !GetRedirectFromConfig(cvm.CategoryId);
#if SIMPLE
            if (addCat) AddCategory(cvm);
#else
            AddProductsInCategory(cvm, addCat);
#endif
            foreach (var cat in cvm.Items)
                AddCategoriesRecursive(cat);
        }

        private async void AddProductsInCategory(CategoryViewModel cvm, bool addCat)
        {
            var sr = await _catViewModelBuilder.SearchProductByCategoryAsync(cvm.CategoryId, new PagingOptions {  Page = 1, PageSize = 100000 }).ConfigureAwait(false);
            if (sr != null && sr.Products != null && sr.Products.Product != null)
            {
                if (sr.Products.Product.Length > 0 && addCat)
                    AddCategory(cvm);

                foreach (var product in sr.Products.Product)
                    AddProduct(product);
            }
        }

        private void AddProduct(Product product)
        {
            bool noIndex;
            var noIndexAttr = product.CustomAttributes.ValueByName("NoIndex");
            if (noIndexAttr == null || !bool.TryParse(noIndexAttr, out noIndex))
                noIndex = false;

            if (!product.DisplayableProduct || noIndex) return;

            object pp;
            var url = _linkGenerator.GenerateProductLink(product.Id, out pp);
            var productPage = pp as ProductPage;

            if (productPage != null && !productPage.IncludeInSitemap()) return;

            var sme = new SiteMapEntry
            {
                Title = product.DisplayName,
                Url = url,
                LastModified = DateTime.Now
            };

            if (productPage != null)
            {
                sme.LastModified = productPage.Published;
                sme.ChangeFrequency = productPage.ChangeFrequency;
                sme.Priority = productPage.Priority;
                sme.Title = productPage.Title;
            }
            Add(sme);
        }

        private void AddCategory(CategoryViewModel cvm)
        {
            object cp;
            var url = _linkGenerator.GenerateCategoryLink(cvm.CategoryId, null, out cp);
            var categoryPage = cp as CatalogPage;

            if (categoryPage != null && !categoryPage.IncludeInSitemap()) return;

            var sme = new SiteMapEntry
            {
                Title = cvm.DisplayName,
                Url = url,
                LastModified = DateTime.Now
            };

            if (categoryPage != null)
            {
                sme.LastModified = categoryPage.Published;
                sme.ChangeFrequency = categoryPage.ChangeFrequency;
                sme.Priority = categoryPage.Priority;
                sme.Title = categoryPage.Title;
            }
            Add(sme);
        }

        // virtual for testing
        protected virtual ContentItem GetRootItem(string url)
        {
            return Context.Current.UrlParser.FindPath(url).StopItem;
        }

        private void AddContentPages(string url)
        {
            var root = GetRootItem(url);
            // if (root is LanguageRoot) root = root.Parent; // TODO rewrite url to be below LanguageRoot

            foreach (var item in GetDescendants(root))
            {
                if (!item.IsPublished()) continue;

                var page = item as PageModelBase;
                if (page != null && !page.IncludeInSitemap()) continue;
                // special case of generic ProductPage and CatalogPage
                //   have to configure generic page in N2 to include in sitemap in order to get all the products and categories to be included in sitemap
                //   but don't want the generic page naked without reference to an ID to be in the sitemap
                var productPage = item as ProductPage;
                if ((productPage != null && string.IsNullOrEmpty(productPage.ProductID)) ||
                    (item is CatalogPage && string.IsNullOrEmpty(((CatalogPage) item).CategoryID)))
                    continue;

                if (productPage != null)
                {
                    long pid;
                    if (long.TryParse(productPage.ProductID, out pid))
                    {
                        var prod = _catalogApi.GetProductAsync(_catalogApi.GetProductUri(pid)).Result;
                        if (prod != null && prod.CustomAttributes.ValueByName("NoIndex", false))
                            continue;
                    }
                }

                var se = new SiteMapEntry
                {
                    Title = item.Title,
                    Url = item.Url,
                    LastModified = item.Published,
                };

                if (page != null)
                {
                    se.ChangeFrequency = page.ChangeFrequency;
                    se.Priority = page.Priority;
                }

                Add(se);
            }	
        }

        // virtual for testing
        protected virtual CategoryRedirect GetCategoryRedirect(long categoryId)
        {
            return CategoryRedirectConfig.GetCategoryRedirect(categoryId);
        }

        private bool GetRedirectFromConfig(long categoryId) 
        {
            var catRed = GetCategoryRedirect(categoryId);
            if (catRed == null) return false;

            long catredId;
            if (!long.TryParse(catRed.Value, out catredId))
                return false;

            switch (catRed.ToWhat)
            {
                case CategoryRedirectToWhat.CategoryId:
                    if (catredId != categoryId)
                        return true;
                    break;
                case CategoryRedirectToWhat.ProductId:
                    return true;
                case CategoryRedirectToWhat.Path:
                {
                    // gen url based on store link minus last segment for store itself
                    var storeLink = _linkGenerator.GenerateStoreLink();
                    if (!string.IsNullOrEmpty(storeLink))
                    {
                        var lastSlashIdx = storeLink.LastIndexOf('/');
                        if (lastSlashIdx >= 0)
                            return true;
                    }
                }
                    break;
            }
            return false;
        }

        private IEnumerable<ContentItem> GetDescendants(ContentItem parent)
        {
            if (_seenChild.Contains(parent.ID)) yield break;
            _seenChild.Add(parent.ID);
             
            var children = new List<ContentItem>();
            children.AddRange(parent.Children.FindPages());
            if (parent is LanguageRoot)
                children.AddRange(parent.Parent.Children.FindPages().Where(p => p.ID != parent.ID));

            foreach (var child in children)
            {
                if (child is IRedirect) continue; // includes StartPage
                if (child is ISystemNode) continue;

                if (!child.Visible) continue;
                if (!child.IsAuthorized(new GenericPrincipal(new GenericIdentity(""), null))) continue;

                if (!(child is ILanguage) && (child is PageModelBase && ((PageModelBase) child).IncludeInSitemap()))
                    yield return child;

                foreach (var descendant in GetDescendants(child))
                    yield return descendant;
            }
        }
    }
}
