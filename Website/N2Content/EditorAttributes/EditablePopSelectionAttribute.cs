//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using N2;
using N2.Details;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
	[AttributeUsage(AttributeTargets.Property)]
    public class EditablePopSelectionAttribute : AbstractEditableAttribute
	{
		public EditablePopSelectionAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

        public override bool UpdateItem(ContentItem item, System.Web.UI.Control editor)
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

            return true;
        }

        public override void UpdateEditor(ContentItem item, System.Web.UI.Control editor)
        {
            // here we update the editor control to reflect what was saved
            string selectedId = Utility.Convert<string>(item[Name]) ?? DefaultValue as string;
            if (selectedId == null) return;
            var list = editor as DropDownList;
            if (list != null)
                list.SelectedValue = selectedId;
        }

        protected override System.Web.UI.Control AddEditor(System.Web.UI.Control container)
        {
            var list = CreateList(LoadPopNames());
            container.Controls.Add(list);
            return list;
        }

        private static IEnumerable<string> LoadPopNames()
	    {
            try
            {
                ShopperApiClientHelperForN2Admin.AssureLimitedAuthentication(false);
                return Context.Current.Container.Resolve<IOffersApi>().GetPointOfPromotionNamesAsync().Result;
            }
            catch
            {   // TODO - better error handling, e.g. show an input box 
                return new string[0];
            }
	    }

	    private DropDownList CreateList(IEnumerable<string> popNames)
	    {
            // here we create the editor control
	        var list = new DropDownList
	        {
	            ID = Name,
	            DataTextField = "PopName",
	            DataValueField = "PopName",
	            DataSource = popNames.Select(p => new { PopName = p })
	        };
	        list.DataBind();
	        return list;
	    }
	}
}
