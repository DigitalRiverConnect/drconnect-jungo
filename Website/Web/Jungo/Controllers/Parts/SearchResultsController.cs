//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  12/13/2012  HGodinez           Created
// 

using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;
using N2.Web;
using Pricing = Jungo.Models.ShopperApi.Catalog.Pricing;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(SearchResultsPart))]
    public class SearchResultsController : PartControllerBase<SearchResultsPart>
    {
        public SearchResultsController(IRequestLogger logger) : base(logger)
        {
            
        }

        public override ActionResult Index()
        {
            var searchResult = IsManaging() ? GetDemoSearchResult() : WebSession.Current.Get<SearchPageViewModel>(WebSession.SearchResultSlot);

            return PartialView(searchResult);
        }

        private static SearchPageViewModel GetDemoSearchResult()
        {
            var ret = new SearchPageViewModel
            {
                CurrentPage = 1,
                EnableFacets = false,
                PageHasProdResultsPart = false,
                PageSize = 20,
                TotalResults = 50,
                KeyWords = "Lorem Ipsum",
                Title = "Lorem Ipsum",
                Products = new ProductsWithRanking()
            };

            var product = new ProductWithRanking[20];
            for (var i = 0; i < 20; i++)
            {
                product[i] = NewProduct();
            }

            ret.Products.Product = product;

            return ret;
        }

        private static ProductWithRanking NewProduct()
        {
            return new ProductWithRanking
            {
                DisplayName = "Lorem Ipsum",
                LongDescription =
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam commodo diam ut purus aliquam, sit amet sollicitudin nisl posuere.",
                ThumbnailImage = "http://placehold.it/150x150",
                Pricing = new Pricing
                {
                    TotalDiscountWithQuantity = new MoneyAmount { Currency = "$", Value = 2 },
                    FormattedSalePriceWithQuantity = "$5.00"
                }
            };
        }
    }
}
