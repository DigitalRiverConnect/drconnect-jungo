//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: 2009/08/15 17:40:01 $
// Modified by: $Author: ALiu $
// Revision: $Revision: 1.1 $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  03/21/2013  BRichan        Created
//  

using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
	[PartDefinition("API Product Results",
		Description = "Results of a product search.", 
		SortOrder = 220,
		IconUrl = "~/Content/img/icons/docs_16.png")]
	[RestrictPageTypes(typeof(CatalogPage))]
	public class ProductResultsPart : PartModelBase
	{
        [EditableCheckBox(Title = "Enable Facets", Name = "EnableFacets", SortOrder = 100, CheckBoxText = "")]
        public bool EnableFacets
        {
            get { return ((bool?)GetDetail("EnableFacets") ?? false); }
            set { SetDetail("EnableFacets", value, false); }
        }

        [EditableText(Title = "List price ranges (leave blank to show all)", SortOrder = 200, HelpText = "Comma-delimited list of text attribute names")]
        public string ListPriceRanges
        {
            get { return ((string)GetDetail("ListPriceRanges") ?? ""); }
            set { SetDetail("ListPriceRanges", value, ""); }
        }
	}

	[PartDefinition("API Search Results",
		Description = "Results of a text search.",
		SortOrder = 221,
		IconUrl = "~/Content/img/icons/docs_16.png")]
    [RestrictPageTypes(typeof(SearchPage))]
    public class SearchResultsPart : PartModelBase
	{

	}

    [PartDefinition("API Product Details",
		Description = "Details of product.",
		SortOrder = 222,
		IconUrl = "~/Content/img/icons/docs_16.png")]
    [RestrictPageTypes(typeof(ProductPage))]
    public class ProductDetailsPart : PartModelBase
	{
	}

    [PartDefinition("API Interstitial Product Details",
        Description = "Details of product on Interstitial Page.",
        SortOrder = 222,
        IconUrl = "~/Content/img/icons/cart16.png")]
    [RestrictPageTypes(typeof(ShoppingCartInterstitialPage))]
    public class InterstitialProductDetailsPart : PartModelBase
    {
    }

    [PartDefinition("API Shopping Cart",
    Description = "Shopping Cart.",
    SortOrder = 223,
    IconUrl = "~/Content/img/icons/cart16.png")]
    [RestrictPageTypes(typeof(ShoppingCartPage))]
    public class ShoppingCartPart : PartModelBase
    {
    }

    [PartDefinition("Shopping Cart Summary",
        Description = "Shopping Cart Summary.",
        SortOrder = 223,
        IconUrl = "~/Content/img/icons/cart16.png")]
    public class ShoppingCartSummaryPart : PartModelBase
    {
    }

	[PartDefinition("Mini Cart",
		Description = "Mini-Cart to display Cart Count.",
		SortOrder = 224,
		IconUrl = "~/Content/img/icons/cart16.png")]
	public class MiniCartPart : PartModelBase
	{
	}
}
