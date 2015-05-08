using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using N2;
using N2.Engine;
using N2.Web.Mvc;
using N2.Web.Parts;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.PartsAdapters
{
    public class PartAdapterRenderInfo
    {
        public string Path { get; set; }
        public object Model { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// Implements "Recursive" zones functionality.
    /// </summary>
    [Adapts(typeof(PageModelBase))]
    public class RecursiveZonePartsAdapterBase : PartsAdapter
    {
        private readonly IRequestLogger _logger;
        private readonly ICatalogApi _catalogApi;

        public RecursiveZonePartsAdapterBase(IRequestLogger logger)
            : this(logger, null)
        {
        }

        public RecursiveZonePartsAdapterBase(IRequestLogger logger, ICatalogApi catalogApi)
        {
            _logger = logger;
            _catalogApi = catalogApi;
        }

        public override System.Collections.Generic.IEnumerable<ContentItem> GetParts(ContentItem parentItem, string zoneName, string @interface)
        {
            var items = base.GetParts(parentItem, zoneName, @interface);
            ContentItem grandParentItem = parentItem;
            if (zoneName.StartsWith("Recursive") && grandParentItem is PageModelBase && !(grandParentItem is LanguageIntersection))
            {
                // merge in inherited items, TODO tbd. how to override 
                items = items.Union(!parentItem.VersionOf.HasValue ? GetParts(parentItem.Parent, zoneName, @interface) : GetParts(parentItem.VersionOf.Parent, zoneName, @interface));
            }
            return items;
        }

        public override void RenderPart(HtmlHelper html, ContentItem part, TextWriter writer = null)
        {
            var currentPath = html.ViewContext.RouteData.CurrentPath();
            try
            {
                var newPath = currentPath.Clone(currentPath.CurrentPage, part);
                html.ViewContext.RouteData.ApplyCurrentPath(newPath);

                PrepareOrRender(html, part, writer);
            }
            finally
            {
                html.ViewContext.RouteData.ApplyCurrentPath(currentPath);
            }
        }

        protected virtual void PrepareOrRender(HtmlHelper html, ContentItem part, TextWriter writer = null)
        {
            var av = PrepareRenderInfo(html, part);

            if (av == null)
                base.RenderPart(html, part, writer);

            if (string.IsNullOrEmpty(av.Path))
                html.ViewContext.Writer.Write(av.Content);
            else
                html.RenderPartial(av.Path, av.Model);
        }

        public virtual PartAdapterRenderInfo PrepareRenderInfo(HtmlHelper html, ContentItem part)
        {
            return null;
        }

        protected Product GetProduct(HtmlHelper html, IProductPart item)
        {
            if (item == null) return null;

            Product product = null;

            var currentController = html.ViewContext.Controller as INimbusContentController;
            try
            {
                long pid;
                if (!long.TryParse(item.Product, out pid))
                    return null;

                if (currentController == null ||
                     (!currentController.Products.TryGetValue(pid, out product) &&
                      !currentController.BogusProductIds.Contains(pid)))
                    product = _catalogApi.GetProductAsync(_catalogApi.GetProductUri(pid)).Result;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to retrieve product with product ID {0}", item.Product);
                product = null;
            }

            return product;
        }

        protected IRequestLogger Logger
        {
            get { return _logger; }
        }
    }
}