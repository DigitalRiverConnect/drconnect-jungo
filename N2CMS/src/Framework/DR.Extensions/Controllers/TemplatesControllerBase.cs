// copied from N2.Templates.Mvc.Controllers 
// OpenSource 

using System;
using System.Web.Mvc;
using System.Web.Routing;
using N2.Web.Mvc;
using N2.Web.UI;

namespace N2.Controllers
{
	/// <summary>
	/// Base class with default handling of index action and other useful helpers.
	/// </summary>
	/// <typeparam name="T">The type of content this controller handles.</typeparam>
	public abstract class TemplatesControllerBase<T> : ContentController<T> where T : ContentItem
	{
	    protected TemplatesControllerBase()
	    {
	        TempDataProvider = null; // or Dummy ?
		}

		//// Actions ////

		/// <summary>By default the templates index method renders a (partial) view located in (see below).</summary>
		/// <returns></returns>
		public override ActionResult Index()
		{
		    return CurrentItem.IsPage
                ? View(string.Format("PageTemplates/{0}", CurrentItem.GetContentType().Name), CurrentItem)
                : PartialView(string.Format("PartTemplates/{0}", CurrentItem.GetContentType().Name), CurrentItem);
		}

	    //// View overloads ////

		/// <summary>
		/// Overrides/hides the most common method of rendering a View and calls View or PartialView depending on the type of the model
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		protected new ViewResultBase View(object model)
		{
			var item = ExtractFromModel(model) ?? CurrentItem;

			if (item != null && !item.IsPage)
				return PartialView(model);

			return base.View(model);
		}

		/// <summary>
		/// Overrides/hides the most common method of rendering a View and calls View or PartialView depending on the type of the model
		/// </summary>
		/// <param name="viewName"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		protected new ViewResultBase View(string viewName, object model)
		{
			var item = ExtractFromModel(model) ?? CurrentItem;

			if (item != null && !item.IsPage)
				return PartialView(viewName, model);

			return base.View(viewName, model);
		}

		protected override ViewResult View(IView view, object model)
		{
			CheckForPageRender(model);

			return base.View(view, model ?? CurrentItem);
		}

		protected override ViewResult View(string viewName, string masterName, object model)
		{
			CheckForPageRender(model);

			return base.View(viewName, masterName, model ?? CurrentItem);
		}

		/// <summary>Renders a script that loads the given action asynchronously from the client.</summary>
		/// <param name="actionName">The name of the action to call.</param>
		/// <returns>A content result with a load action script.</returns>
		protected virtual ActionResult AsyncView(string actionName)
		{
			return AsyncView(actionName, null);
		}

		/// <summary>Renders a script that loads the given action asynchronously from the client.</summary>
		/// <param name="actionName">The name of the action to call.</param>
		/// <param name="routeParams">Additional parameters for the async load request.</param>
		/// <returns>A content result with a load action script.</returns>
		protected virtual ActionResult AsyncView(string actionName, object routeParams)
		{
			var values = new RouteValueDictionary(routeParams);
			values[ContentRoute.ActionKey] = actionName;

			return AsyncView(values);
		}

		/// <summary>Renders a script that loads the given action asynchronously from the client.</summary>
		/// <param name="routeParams">The route parameters to use for building the url.</param>
		/// <returns>A content result with a load action script.</returns>
		protected virtual ActionResult AsyncView(RouteValueDictionary routeParams)
		{
			if (!routeParams.ContainsKey(ContentRoute.ContentPartKey))
				routeParams[ContentRoute.ContentPartKey] = CurrentItem.ID;
			
			var id = "part_" + routeParams[ContentRoute.ContentPartKey];
			Web.Url url = CurrentPage.Url;
			url = url.UpdateQuery(routeParams);

			return Content(string.Format(@"<div id='{0}' class='async loading'></div><script type='text/javascript'>//<![CDATA[
jQuery('#{0}').load('{1}');//]]></script>", id, url));
		}

		//// Helpers ////

		private void CheckForPageRender(object model)
		{
			var item = ExtractFromModel(model) ?? CurrentItem;

			if (item != null && !item.IsPage)
				throw new InvalidOperationException(@"Rendering of Parts using View(..) is no longer supported. Use PartialView(..) to render this item.
			
- Item of type " + item.GetContentType() + @"
- Controller is " + GetType() + @"
- Action is " + ViewData[ContentRoute.ActionKey]);
		}

		private static ContentItem ExtractFromModel(object model)
		{
			var item = model as ContentItem;

			if (item != null)
				return item;

			var itemContainer = model as IItemContainer;

			return itemContainer != null ? itemContainer.CurrentItem : null;
		}
	}
}