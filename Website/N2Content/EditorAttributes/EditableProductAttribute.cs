//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
//  

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using Jungo.Models.ShopperApi.Catalog;
using N2;
using N2.Details;

// custom editor for product ids using a dropdown 

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
	public class EditableProductAttribute : AbstractEditableAttribute
	{
		/// <summary>Initializes a new instance of the EditableTextBoxAttribute class.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		public EditableProductAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		protected override Control AddEditor(Control container)
		{
			IEnumerable<ProductBrief> productBriefs;
			try
			{
                ShopperApiClientHelperForN2Admin.AssureLimitedAuthentication(false);
                productBriefs = Context.Current.Container.Resolve<ICatalogApi>().GetProductBriefsAsync().Result;
			}
			catch
			{   // TODO - better error handling, e.g. show an input box 
                productBriefs = new ProductBrief[0];
			}

			// here we create the editor control and add it to the page
			var list = new DropDownList
						   {
							   ID = Name,
							   DataTextField = "IdAndName",
							   DataValueField = "Id",
                               DataSource = productBriefs
						   };
			list.DataBind();
			container.Controls.Add(list);
			return list;
		}

		public override void UpdateEditor(ContentItem item, Control editor)
		{
			// here we update the editor control to reflect what was saved
			string selectedId = Utility.Convert<string>(item[Name]) ?? DefaultValue as string;
			if (selectedId != null)
			{
				var list = editor as DropDownList;
				if (list != null)
					list.SelectedValue = selectedId;
			}
		}

		public override bool UpdateItem(ContentItem item, Control editor)
		{
			// here we update the item from dropdown selection
			var list = editor as DropDownList;
			var selectedId = "";
			if (list != null)
				selectedId = list.SelectedValue;

			var previouslySelected = Utility.Convert<string>(item[Name]) ?? DefaultValue as string;
			if (previouslySelected != null && previouslySelected == selectedId)       
				return false; // no change

			item[Name] = selectedId;
			item["Title"] = ""; // remove custom title - TODO

			return true;
		}
	}
}