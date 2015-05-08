using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Resources;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2;
using N2.Web;
using N2.Web.Mvc;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    public class ContentControllerBase<T> : ContentController<T>, INimbusContentController
        where T : ContentItem
    {
        private readonly ICatalogApi _catalogApi;
#if SAVE_VIEW_MODEL
        private readonly string _saveVmDir;
#endif

        private ControllerContentHelper _content;
        private string[] _arguments;

        protected readonly IRequestLogger Logger;
        protected readonly ILinkGenerator LinkGenerator;

        public ContentControllerBase(ICatalogApi catalogApi)
        {
            _catalogApi = catalogApi;
            TempDataProvider = null;

#if SAVE_VIEW_MODEL
            _saveVmDir = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/SavedViewModels");
            if (!Directory.Exists(_saveVmDir))
                Directory.CreateDirectory(_saveVmDir);
#endif
        }

        public ContentControllerBase(IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
        {
            _catalogApi = catalogApi;

            Logger = logger;
            LinkGenerator = linkGenerator;

#if SAVE_VIEW_MODEL
            _saveVmDir = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/SavedViewModels");
            if (!Directory.Exists(_saveVmDir))
                Directory.CreateDirectory(_saveVmDir);
#endif
        }

        public override ActionResult Index()
        {
            AssertProductsLoaded();

            return Arguments.Length > 0 ? NotFound() : base.Index();
        }

        protected void AssertProductsLoaded(long? pageProductId = null)
        {
            HttpContext.AssertProductsLoaded(_catalogApi, CurrentItem, pageProductId);
        }

        public Dictionary<long, Product> Products
        {
            get { return HttpContext.GetReqScopeProducts(); }
        }

        public List<long> BogusProductIds
        {
            get { return HttpContext.GetReqScopeBogusProductIds(); }
        }

        // overridable for testing, to break dependency on N2 context and request context
        protected virtual bool IsManaging
        {
            get { return this.IsManaging(); }
        }

        // break dependency on WebSession
        private string _sessionIdForTesting = string.Empty;
        public string SessionId
        {
            protected get { return string.IsNullOrEmpty(_sessionIdForTesting) ? WebSession.Current.SessionId : _sessionIdForTesting; }
            set { _sessionIdForTesting = value; }
        }

        // overridable for testing, to break dependency on WebSession, which depends on HttpContext which can't be mocked
        protected virtual TS WebSessionGet<TS>(string name) where TS : new()
        {
            return WebSession.Current.Get<TS>(name);
        }

        // overridable for testing, to break dependency on WebSession, which depends on HttpContext which can't be mocked; see below
        protected virtual void WebSessionSet<TS>(string name, TS value)
        {
            WebSession.Current.Set(name, value);
        }

        // conveniences for getting/setting security token
        protected virtual SecurityToken GetSecurityTokenFromWebSession() { return WebSessionGet<SecurityToken>(WebSession.SecurityTokenSlot); }
        protected virtual void SetSecurityTokenInWebSession(SecurityToken securityToken) { WebSessionSet(WebSession.SecurityTokenSlot, securityToken); }

        // break dependency on HttpContext:
        //   a) The HttpContext in the getter below is actually an HttpContextBase
        //   b) ToHttpContext() accesses HttpContext.ApplicationBase
        //   c) the it accesses ApplicationBase.Context, which is of type HttpContext (not HttpContextBase)
        //   d) the type HttpContext (not HttpContextBase) is sealed and can't be mocked
        //   e) ApplicationBase.Context has no setter
        // therefore, HttpContext.ToHttpContext() can't be mocked, but relies on runtime magic of MVC not available during unit testing
        private Guid? _requestIdForTesting;
        public Guid? RequestId
        {
            protected get { return _requestIdForTesting ?? HttpContext.ToHttpContext().GetId(); }
            set { _requestIdForTesting = value; }
        }

        internal static string PathOverrideKey
        {
            get { return "Override." + PathData.PathKey; }
        }

        /// <summary>Access to commonly used APIs.</summary>
        public ControllerContentHelper ContentHelper
        {
            get
            {
                return _content ??
                       (_content =
                        new ControllerContentHelper(() => RouteExtensions.GetEngine(RouteData), () => RouteData.CurrentPath()));
            }
            set { _content = value; }
        }

        protected string[] Arguments
        {
            get
            {
                return _arguments ?? (_arguments = string.IsNullOrEmpty(Content.Current.Path.Argument)
                                                         ? new string[0]
                                                         : Content.Current.Path.Argument.Split('/'));
            }
        }

#if SAVE_VIEW_MODEL
        protected void SaveViewModel<T>(T viewModel, string name)
        {
            var dcs = new DataContractSerializer(typeof(T));
            var fileName = string.Concat(_saveVmDir, @"\", name, ".xml");
            if (System.IO.File.Exists(fileName)) System.IO.File.Delete(fileName);
            var s = System.IO.File.Create(fileName);
            dcs.WriteObject(s, viewModel);
            s.Flush();
            s.Close();
        }
#endif

        protected static string GetPageTitleFromCurrentItem(ContentItem currentItem)
        {
            var page = currentItem as PageModelBase;
            if (page != null && !string.IsNullOrEmpty(page.PageTitle))
                return page.PageTitle;
            return string.Empty;
        }

        protected string GetPageTitleFromCurrentItem()
        {
            return GetPageTitleFromCurrentItem(CurrentItem);
        }

        protected const string ResourceTypePageTitle = "PageTitle";
        private const string ResourceKeyPageTypePrefix = "Pagetype_";

        protected static string GetPageTitleFromResource(string pageType = null)
        {
            var key = ResourceKeyPageTypePrefix;
            if (!string.IsNullOrEmpty(pageType))
                key += pageType;
            else
            {
                key += typeof (T).Name;
                if (key.EndsWith("Page"))
                    key = key.Remove(key.Length - 4);
            }
            var title = Res.ResourceManager.GetString(ResourceTypePageTitle + "_" + key);
            return !string.IsNullOrEmpty(title) ? title : Res.PageTitle_PagetypeAllOthers;
        }

        protected static string GetPageTitle(ContentItem currentItem, string pageType = null)
        {
            var title = GetPageTitleFromCurrentItem(currentItem);
            if (string.IsNullOrEmpty(title))
                title = GetPageTitleFromResource(pageType);
            return title;
        }

        protected string GetPageTitle(string pageType = null)
        {
            return GetPageTitle(CurrentItem, pageType);
        }

        protected void SetPageTitle(string title)
        {
            ViewBag.Title = title;
        }

        public static void SetPageTitle(dynamic viewBag, string title)
        {
            viewBag.Title = title;
        }

        protected void SetPageTitleSimple()
        {
            SetPageTitle(GetPageTitle());
        }

        public static void SetPageTitleSimple(ContentItem currentItem, dynamic viewBag)
        {
            SetPageTitle(viewBag, GetPageTitle(currentItem));
        }

        protected void SetPageTitleOverrideResKey(string pageTypeKey)
        {
            SetPageTitle(GetPageTitle(pageTypeKey));
        }

        public virtual ActionResult NotFound()
        {
            Response.Status = "404 Not Found";
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.Message = "Sorry, there was a problem with your request.";
            return View("NotFound");
        }

        protected void SetPageMetaData(PageViewModelBase vm)
        {
            var startPage = CmsFinder.FindStartPageOf(CurrentItem) as StartPage;
            var item = CurrentItem as PageModelBase;
            if (item != null && vm != null)
            {
                if (!string.IsNullOrEmpty(item.Description))
                    vm.SeoMetaDescription = item.Description;
                if (!string.IsNullOrEmpty(item.Keywords))
                    vm.SeoMetaKeywords = item.Keywords;

                vm.Metadata.TwitterTitle = !(string.IsNullOrEmpty(item.TwitterTitle)) ? item.TwitterTitle : vm.SeoTitleTag;
                vm.Metadata.TwitterImageSrc = item.TwitterImageSrc;
                vm.Metadata.TwitterImageWidth = item.TwitterImageWidth;
                vm.Metadata.TwitterImageHeight = item.TwitterImageHeight;
                vm.Metadata.TwitterData1 = item.TwitterData1;
                vm.Metadata.TwitterLabel1 = item.TwitterLabel1;
                vm.Metadata.TwitterData2 = item.TwitterData2;
                vm.Metadata.TwitterLabel2 = item.TwitterLabel2;
                vm.Metadata.TwitterDescription = !string.IsNullOrEmpty(item.TwitterDescription) ? item.TwitterDescription : vm.SeoMetaDescription;
                vm.Metadata.TwitterCard = item.TwitterCard;
                vm.Metadata.OgImage = item.OgImage;
                vm.Metadata.OgType = item.OgType;
                vm.Metadata.OgTitle = !string.IsNullOrEmpty(item.OgTitle) ? item.OgTitle : vm.PageTitle;
                vm.Metadata.OgDescription = !string.IsNullOrEmpty(item.OgDescription) ? item.OgDescription : vm.SeoMetaDescription;
            }
            if (startPage != null && vm != null)
            {
                vm.Metadata.FacebookAppId = startPage.FacebookAppId;
                vm.Metadata.TwitterSite = startPage.TwitterSite;
                vm.Metadata.ApplicationName = startPage.ApplicationName;
                vm.Metadata.OgSiteName = startPage.OgSiteName;
            }
        }

        protected override void ProcessOutputCache(ResultExecutingContext filterContext)
        {
            var cp = CurrentItem as CachingPageBase;
            if (cp == null)
            {
                base.ProcessOutputCache(filterContext);
            }
            else if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.Response.SetValidUntilExpires(TimeSpan.FromSeconds(cp.CacheMaxAge));
            }
        }

        protected ICatalogApi CatalogApi { get { return _catalogApi; } }
    }
}
