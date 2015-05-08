using System;
using System.Linq;
using System.Web.UI.WebControls;
using N2.Security;
using N2.Persistence;
using N2.Edit.Versioning;
using N2.Web;

namespace N2.Edit.Versions
{
	[ToolbarPlugin("VERS", "versions", "{ManagementUrl}/Content/Versions/Default.aspx?{Selection.SelectedQueryKey}={selected}", ToolbarArea.Preview, Targets.Preview, "{ManagementUrl}/Resources/icons/book_previous.png", 90, 
        ToolTip = "versions", 
        GlobalResourceClassName = "Toolbar",
		RequiredPermission = Permission.Write,
		Legacy = true)]
	[ControlPanelPendingVersion("There is a newer unpublished version of this item.", 200)]
	public partial class Default : Web.EditPage
	{
        private ContentItem publishedItem;

        private IPersister persister;
        private IVersionManager versioner;
	    private ISecurityManager security;

		protected override void OnInit(EventArgs e)
		{
            Page.Title = string.Format("{0}: {1}", GetLocalResourceString("VersionsPage.Title", "Versions"), Selection.SelectedItem.Title);

			persister = Engine.Persister;
			versioner = Engine.Resolve<IVersionManager>();
		    security = Engine.SecurityManager;

			bool isVersionable = versioner.IsVersionable(Selection.SelectedItem);
            cvVersionable.IsValid = isVersionable;

			publishedItem = Selection.SelectedItem.VersionOf.Value ?? Selection.SelectedItem;

			base.OnInit(e);
		}

		protected void gvHistory_RowCommand(object sender, GridViewCommandEventArgs e)
		{
            ContentItem currentVersion = Selection.SelectedItem;
			int versionIndex = Convert.ToInt32(e.CommandArgument);
			if (e.CommandName == "Publish")
			{
                versioner.PublishVersion(persister, currentVersion, versionIndex);
			    Refresh(currentVersion, ToolbarArea.Both);
			}
			else if (e.CommandName == "Delete") // why not delete draft && currentVersion.VersionIndex != versionIndex)
			{                
                // delete a version, not the item!                
                ContentItem item = versioner.GetVersion(currentVersion, versionIndex);
                if (CanDeleteVersion(item))
			    {
			        versioner.DeleteVersion(item);
					Refresh(currentVersion, ToolbarArea.Both);
			    }
			}
		}

	    protected void gvHistory_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{

		}

		public class VersionInfoViewModel
		{
			public int ID { get; set; }
			public string Title { get; set; }
			public ContentState State { get; set; }
			public string IconUrl { get; set; }
			public DateTime? Published { get; set; }
			public DateTime? Expires { get; set; }
            public DateTime Updated { get; set; }
			public int VersionIndex { get; set; }
			public string SavedBy { get; set; }
			public ContentItem Content { get; set; }
		    public string UserIconUrl {
                get { return "{IconsUrl}/user.png"; } // TODO use actual avatar
		    }

            // TODO implement IsXXX helpers here to simplify code in view
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			var versions = versioner.GetVersionsOf(publishedItem)
                .Select(v => new VersionInfoViewModel
                {
                    ID = v.ID,
                    Title = v.Title,
                    State = v.State,
                    IconUrl = Url.ResolveTokens(v.IconUrl ?? "{IconsUrl}/page.png"),
                    Updated = v.Updated,
                    Published = v.Published, Expires = v.Expires, VersionIndex = v.VersionIndex, SavedBy = v.SavedBy, Content = v })
				.ToList();			
			
#if FIXUP_HISTORY
            // SW: hmm - I think it is a bad idea to make up this data here, it should be correct in the database
            DateTime? previousExpired = publishedItem.Published;
			foreach (var version in versions.OrderBy(v => v.VersionIndex))
			{
				version.Published = previousExpired;
				previousExpired = version.Expires;
			}
#endif

            gvHistory.DataSource = versions;
			gvHistory.DataBind();
		}


		protected bool CanEdit(object dataItem)
		{
            var item = dataItem as ContentItem;
		    return security.IsAuthorized(User, item, Permission.Write);
		}

		protected bool CanPublish(object dataItem)
		{
            var item = dataItem as ContentItem;
		    return !IsPublished(item) && security.IsAuthorized(User, item, Permission.Publish);
		}

		protected bool CanDeleteVersion(object dataItem)
		{
            var item = dataItem as ContentItem;
            return item != null && (!item.IsPublished() && item.VersionOf.HasValue) &&
		           security.IsAuthorized(User, item, Permission.Administer);
		}

		protected bool IsPublished(object dataItem)
		{
			var item = dataItem as ContentItem;
			return item != null && (publishedItem.Equals(item) && item.IsPublished());
		}

		protected bool IsFuturePublished(object dataItem)
		{
			var item = dataItem as ContentItem;
            return item != null && (publishedItem.Equals(item) && item.IsFuturePublished());
		}

        protected string VersionUrl(object dataItem)
        { 
            var item = dataItem as ContentItem;
            if (item != null)
                return ResolveUrl("{ManagementUrl}/version.n2.ashx") + "?action=version&path=" + item.Path + "&versionIndex=" + item.VersionIndex;

            return "";
        }

	    protected string PublishString(object dataItem)
	    {
	        var item = dataItem as ContentItem;
	        if (item != null)
	        {
	            if (item.State == ContentState.Waiting)
	                return GetLocalResourceString("FuturePublish.Label", "on ") + item.FuturePublishDate;

	            if (item.Published.HasValue)
	                return item.Published.Value.ToString();
	        }
	        return "";
	    }
	}
}
