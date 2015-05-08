using System.Collections.Generic;
using System.Linq;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Definitions;
using N2.Details;
using N2.Integrity;
using N2.Models;
using N2.Web;
using N2.Web.UI;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    /// <summary>
    /// Base implementation for pages.
    /// </summary>
    [WithEditableTitle(ContainerName = Defaults.Containers.Content, SortOrder = 100)]
    [WithEditableName(SortOrder = 100)]
    [TabContainer(Defaults.Containers.Content, "Content", 1000)]
    [TabContainer(Defaults.Containers.Metadata, "Metadata", 1100)]
    [TabContainer(Defaults.Containers.Advanced, "Advanced", 1200)]
    [RestrictParents(typeof(IPage))]
    public abstract class PageModelBase : ContentBase, IPage
    {
        private readonly object _pidLock = new object();

        public virtual bool IncludeInSitemap()
        {
            return !ExcludeFromSitemap;
        }

        public override PathData FindPath(string remainingUrl)
        {
            var path = base.FindPath(remainingUrl);

            if (path.IsEmpty() && !(this is LanguageRoot))
            {
                var languageRoot = Find.ClosestOf<LanguageRoot>(this);
                if (languageRoot != null)
                    path = languageRoot.FindPath(remainingUrl);
            }

            if (path.IsEmpty() && !(this is StartPage))
            {
                var siteRoot = Find.ClosestOf<StartPage>(this);
                if (siteRoot != null)
                    path = siteRoot.FindPath(remainingUrl);
            }

            if (path.IsEmpty() && !(this is LanguageIntersection))
            {
                var languageIntersection = Find.ClosestOf<LanguageIntersection>(this);
                if (languageIntersection != null)
                    path = languageIntersection.FindPath(remainingUrl);
            }

            if (path.IsEmpty() && remainingUrl == HttpContext.Current.Request.ApplicationPath)
                return PathDictionary.GetPath(CmsFinder.FindLanguageIntersection(), string.Empty);

            if (path.IsEmpty() && !(this is LanguageIntersection) && !(this is StartPage) && !(this is LanguageRoot)) //
                path = base.FindPath("Index" + "/" + remainingUrl);

            return path;
        }

        #region Metadata

        // see http://searchenginewatch.com/article/2067564/How-To-Use-HTML-Meta-Tags
        // http://support.google.com/webmasters/bin/answer.py?hl=en&answer=79812
        // http://www.siteground.com/metatag_optimization.htm

        [EditableText("Page Title", 100, ContainerName = Defaults.Containers.Metadata, HelpText = "overrides all other rules for forming html title tag")]
        public virtual string PageTitle { get; set; }

        /// <summary>
        /// The meta tag "description"
        /// </summary>
        [EditableMetaTag(Title = "Description", SortOrder = 101, ContainerName = Defaults.Containers.Metadata,
             HelpText = "This tells the search engine what your page or site is about. Shows in search engine results list.")]
        public virtual string Description { get; set; }

        /// <summary>
        /// The meta tag "keywords"
        /// </summary>
        [EditableMetaTag(Title = "Keywords", SortOrder = 102, ContainerName = Defaults.Containers.Metadata,
             HelpText = "The only search engine that looks at the keywords anymore is Microsoft's Bing. Excessive use can make your site look like SPAM!")]
        public virtual string Keywords { get; set; }

        /// <summary>
        /// The meta tag "robots"
        /// </summary>
        [EditableMetaTag(Title = "Robots", SortOrder = 103, ContainerName = Defaults.Containers.Metadata)]
        public virtual string Robots { get; set; }

        [EditableCheckBox(Title = "Exclude from Sitemap",
            ContainerName = Defaults.Containers.Advanced, SortOrder = 202,
            HelpTitle = "Generated Sitemap", HelpText = "Exclude this page from the Sitemap")]
        public virtual bool ExcludeFromSitemap { get; set; }

        [EditableEnum(Title = "Change Frequency for Sitemap",
            ContainerName = Defaults.Containers.Advanced, SortOrder = 203,
            HelpTitle = "Generated Sitemap", HelpText = "Information to sitemap crawler: how often this page changes.",
            EnumType = typeof(ChangeFrequencyEnum), DefaultValue = ChangeFrequencyEnum.Daily)]
        public virtual ChangeFrequencyEnum ChangeFrequency { get; set; }

        [EditableDropDownDoubleRange(Title = "Sitemap Priority",
            ContainerName = Defaults.Containers.Advanced, SortOrder = 204,
            HelpTitle = "Generated Sitemap", HelpText = "Information to sitemap crawler: this page priority relative to other site pages.",
            MinimumValue = 0.0, MaximumValue = 1.0, DefaultValue = 0.5, Required = true)]
        public virtual double Priority { get; set; }

        /// <summary>
        /// The meta tag "twitter:title"
        /// </summary>
        [EditableMetaTag(Title = "Twitter Title", SortOrder = 301, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterTitle { get; set; }

        /// <summary>
        /// The meta tag "twitter:description"
        /// </summary>
        [EditableMetaTag(Title = "Twitter Description", SortOrder = 302, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterDescription { get; set; }

        /// <summary>
        /// The meta tag "twitter:card"
        /// </summary>
        [EditableMetaTag(Title = "Twitter Card", SortOrder = 303, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterCard { get; set; }

        /// <summary>
        /// The meta tags "twitter:image:src" - currently only relevant for Store Home Page and Product Details Pages
        /// </summary>
        [EditableImageSelection("", Title = "Twitter Image", SortOrder = 304, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterImageSrc
        {
            get { return ((string)GetDetail("TwitterImageSrc") ?? ""); }
            set { SetDetail("TwitterImageSrc", value, ""); }
        }

        /// <summary>
        /// The meta tag "twitter:image:width" - currently only relevant for Store Home Page and Product Details Pages
        /// </summary>
        [EditableMetaTag(Title = "Twitter Image Width", SortOrder = 305, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterImageWidth { get; set; }

        /// <summary>
        /// The meta tag "twitter:image:height" - currently only relevant for Store Home Page and Product Details Pages
        /// </summary>
        [EditableMetaTag(Title = "Twitter Image Height", SortOrder = 306, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterImageHeight { get; set; }

        /// <summary>
        /// The meta tag "twitter:data1" - currently only relevant for Store Home Page and Product Details Pages
        /// </summary>
        [EditableMetaTag(Title = "Twitter Data 1", SortOrder = 307, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterData1 { get; set; }

        /// <summary>
        /// The meta tag "twitter:label1" - currently only relevant for Store Home Page and Product Details Pages
        /// </summary>
        [EditableMetaTag(Title = "Twitter Label 1", SortOrder = 308, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterLabel1 { get; set; }

        /// <summary>
        /// The meta tag "twitter:data2" - currently only relevant for Store Home Page and Product Details Pages"
        /// </summary>
        [EditableMetaTag(Title = "Twitter Data 2", SortOrder = 309, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterData2 { get; set; }

        /// <summary>
        /// The meta tag "twitter:label1" - currently only relevant for Store Home Page and Product Details Pages
        /// </summary>
        [EditableMetaTag(Title = "Twitter Label 2", SortOrder = 310, ContainerName = Defaults.Containers.Metadata)]
        public virtual string TwitterLabel2 { get; set; }

        /// <summary>
        /// The meta tag OpenGraph "og:type"
        /// </summary>
        [EditableMetaTag(Title = "OpenGraph - Type", SortOrder = 405, ContainerName = Defaults.Containers.Metadata)]
        public virtual string OgType { get; set; }

        /// <summary>
        /// The meta tag OpenGraph "og:title"
        /// </summary>
        [EditableMetaTag(Title = "OpenGraph - Title", SortOrder = 410, ContainerName = Defaults.Containers.Metadata)]
        public virtual string OgTitle { get; set; }

        /// <summary>
        /// The meta tag OpenGraph "og:description"
        /// </summary>
        [EditableMetaTag(Title = "OpenGraph - Description", SortOrder = 415, ContainerName = Defaults.Containers.Metadata)]
        public virtual string OgDescription { get; set; }

        /// <summary>
        /// The meta tag OpenGraph "og:image"
        /// </summary>
        [EditableImageSelection("", Title = "OpenGraph - Image", SortOrder = 420, ContainerName = Defaults.Containers.Metadata)]
        public virtual string OgImage
        {
            get { return ((string)GetDetail("OgImage") ?? ""); }
            set { SetDetail("OgImage", value, ""); }
        }

        #endregion

        private long[] _productIds;

        public long[] ProductIds
        {
            get
            {
                if (_productIds == null)
                    FindProductIDsOnPage();

                return _productIds;
            }
        }

        private void FindProductIDsOnPage()
        {
            var pids = new HashSet<long>();
            lock (_pidLock)
            {
                if (_productIds != null) return;

                var children = Children.Where(c => !c.IsPage);
                foreach (var child in children)
                    FindProductIdsOnPageRecursive(child, pids);

                AddToSet(this, pids);

                _productIds = pids.ToArray();
            }
        }

        private static void FindProductIdsOnPageRecursive(ContentItem item, ISet<long> pids)
        {
            AddToSet(item, pids);

            foreach (var child in item.Children)
                FindProductIdsOnPageRecursive(child, pids);
        }

        private static void AddToSet(ContentItem item, ISet<long> pids)
        {
            var ppart = item as IProductPart;
            if (ppart == null || string.IsNullOrEmpty(ppart.Product)) return;
            if (ppart.Product.Contains(','))
            {
                var products = ppart.Product.Split(',');
                foreach (var product in products)
                {
                    long pid;
                    if (long.TryParse(product, out pid))
                        pids.Add(pid);
                }
            }
            else
            {
                long pid;
                if (long.TryParse(ppart.Product, out pid))
                    pids.Add(pid);
            }
        }
    }
}
