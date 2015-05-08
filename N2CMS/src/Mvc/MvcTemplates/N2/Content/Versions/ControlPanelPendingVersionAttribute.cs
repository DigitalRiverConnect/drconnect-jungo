using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2.Edit.Versioning;
using N2.Web;
using N2.Web.UI.WebControls;

namespace N2.Edit.Versions
{
	/// <summary>
	/// Used internally to add the dicard preview button.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ControlPanelPendingVersionAttribute : ControlPanelLinkAttribute
	{
		public ControlPanelPendingVersionAttribute(string toolTip, int sortOrder)
			: base("cpPendingVersion", "{IconsUrl}/book_next_orange.png", null, toolTip, sortOrder, ControlPanelState.Visible)
		{
		}

		public override Control AddTo(Control container, PluginContext context)
		{
			if(!ActiveFor(container, context.State)) return null;
			if (context.Selected == null) return null;
			if (context.Selected.VersionOf.HasValue) return null;

            var draftRepo = Context.Current.Resolve<DraftRepository>();
            var draft = draftRepo.GetDraftInfo(context.Selected);
            if (draft == null)
                return null;
            var latestVersion = draftRepo.FindDrafts(context.Selected).Select(v => v.Version).FirstOrDefault();
			if (latestVersion == null)
				return null;

            Url versionPreviewUrl = Context.Current.GetContentAdapter<NodeAdapter>(latestVersion).GetPreviewUrl(latestVersion);
			versionPreviewUrl = versionPreviewUrl.SetQueryParameter("edit", context.HttpContext.Request["edit"]);

			HyperLink hl = new HyperLink();
			hl.NavigateUrl = versionPreviewUrl;
			hl.Text = GetInnerHtml(context, latestVersion.State == ContentState.Waiting ? "{IconsUrl}/clock_play.png".ResolveUrlTokens() : IconUrl, ToolTip, Title);
			hl.CssClass = "preview";
			hl.ToolTip = Utility.GetResourceString(GlobalResourceClassName, Name + ".ToolTip") ?? context.Format(ToolTip, false);
			container.Controls.Add(hl);

			return hl;
		}
	}
}