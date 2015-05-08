using System;
using N2.Edit.Web;
using N2.Management.Activity;

namespace N2.Management.Content.Activity
{
	[KeepAliveControlPanel]
	public partial class RecentActivity : EditUserControl
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			CurrentItem = Selection.SelectedItem;
			Visible = Engine.Config.Sections.Management.Collaboration.ActivityTrackingEnabled;
		}

		protected override void  OnDataBinding(EventArgs e)
		{		
			var activities = ManagementActivity.GetActivity(Engine, CurrentItem);
			ActivitiesJson = ManagementActivity.ToJson(activities);
			ShowActivities = activities.Count > 0;

			base.OnDataBinding(e);
		}

		public ContentItem CurrentItem { get; set; }

		public string ActivitiesJson { get; set; }

		public bool ShowActivities { get; set; }
	}
}