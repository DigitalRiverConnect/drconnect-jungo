using System.Collections.Generic;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2;
using N2.Engine;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.PartsAdapters
{
    [Adapts(typeof(ListofLinksPart))]
    public class ListOfLinksAdapter : RecursiveZonePartsAdapterBase
    {
        private readonly ILinkGenerator _linkGenerator;

        public ListOfLinksAdapter(ILinkGenerator linkGenerator, ICatalogApi catalogApi, IRequestLogger logger)
            : base(logger, catalogApi)
        {
            _linkGenerator = linkGenerator;
        }

        public override PartAdapterRenderInfo PrepareRenderInfo(HtmlHelper html, ContentItem part)
        {
            var currentItem = (ListofLinksPart)part;

            html.ViewBag.ComponentID = "item_" + part.ID; // unique id for DOM element / CSS


            var viewModel = new ListofLinksViewModel
            {
                Title = currentItem.Title,
                BackgroundColor = currentItem.BackgroundColor,
                ForegroundColor = currentItem.ForegroundColor,
                Subtitle = currentItem.Subtitle,
                TemplateItems = currentItem.TemplateItems,
                UseButton = currentItem.UseButton,
                Links = new List<ListOfLinksItem>()
            };

            foreach (var listofLinksItem in currentItem.Links)
            {
                ListOfLinksItem itemViewModel;

                string targetUrl;
                if (listofLinksItem is ProductListofLinksItem)
                {
                    var item = (ProductListofLinksItem)listofLinksItem;
                    long pid;
                    targetUrl = html.AbsoluteUrlWithHttp(_linkGenerator.GenerateProductLink(long.TryParse(item.Product, out pid) ? pid : (long?)null));
                    itemViewModel = new ListOfLinksItem(
                        item.Title,
                        item.Target,
                        item.LinkText,
                        targetUrl,
                        item.SuppressLinks);
                }
                else if (listofLinksItem is CategoryListofLinksItem)
                {
                    var item = (CategoryListofLinksItem)listofLinksItem;
                    targetUrl = html.AbsoluteUrlWithHttp(_linkGenerator.GenerateCategoryLink(long.Parse(item.Category), item.ForceListPage));
                    itemViewModel = new ListOfLinksItem(
                        item.Title,
                        item.Target,
                        item.LinkText,
                        targetUrl,
                        item.SuppressLinks);
                }
                else if (listofLinksItem is ContentPageListofLinksItem)
                {
                    var item = (ContentPageListofLinksItem)listofLinksItem;
                    targetUrl = html.AbsoluteUrlWithHttp(_linkGenerator.GenerateLinkForNamedContentItem(item.ContentPage));

                    itemViewModel = new ListOfLinksItem(
                        item.Title,
                        item.Target,
                        item.LinkText,
                        targetUrl,
                        item.SuppressLinks);
                }
                else
                {
                    //targetUrl = html.AbsoluteUrlWithHttp(listofLinksItem.TargetUrl);
                    itemViewModel = new ListOfLinksItem(listofLinksItem.Title,
                        listofLinksItem.Target, listofLinksItem.LinkText, listofLinksItem.TargetUrl,
                        listofLinksItem.SuppressLinks);
                }

                // Apply suffix at this point
                if (!string.IsNullOrEmpty(listofLinksItem.UrlSuffix))
                {
                    itemViewModel.TargetUrl = string.Concat(itemViewModel.TargetUrl, listofLinksItem.UrlSuffix);
                }

                viewModel.Links.Add(itemViewModel);
            }

            return new PartAdapterRenderInfo {Path = "ListOfLinks/Index", Model = viewModel};
        }
    }

}