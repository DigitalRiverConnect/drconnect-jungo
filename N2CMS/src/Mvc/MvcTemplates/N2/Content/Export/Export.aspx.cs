using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using N2.Persistence.Serialization;
using N2.Edit.Web;
using N2.Edit;
using N2.Definitions;

namespace N2.Management.Content.Export
{
	public partial class Export : EditPage
	{
		protected AffectedItems ExportedItems;
	    private IEnumerable<IExporter> _exporters;

	    public class ExportFormat
	    {
	        public string Value { get; set; }
            public string Title { get; set; }
	    }

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

		    _exporters = N2.Context.Current.Container.ResolveAll<IExporter>();

		    var enumerable = _exporters as IExporter[] ?? _exporters.ToArray();
		    ddlTypes.DataSource = enumerable
                .SelectMany(d => new[] { new ExportFormat
                {
                    Value = d.GetContentType(),
                    Title = d.GetType().Name
                }});
            ddlTypes.DataBind();
            ddlTypes.Visible = enumerable.Count() > 1;          

			tpImport.NavigateUrl = "Default.aspx?selected=" + Selection.SelectedItem.Path;

			if (Selection.SelectedItem is IFileSystemNode)
			{
				Response.Redirect("../../Files/FileSystem/Export.aspx?path=" + Server.UrlEncode(Selection.SelectedItem.Path) + "#ctl00_ctl00_Frame_Content_tpExport");
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			ExportedItems.CurrentItem = Selection.SelectedItem;
			tpExport.DataBind();
		}

		protected void btnExport_Command(object sender, CommandEventArgs e)
		{
			var options = ExportOptions.Default;
			if (chkDefinedDetails.Checked)
				options |= ExportOptions.OnlyDefinedDetails;
			if (chkAttachments.Checked)
				options |= ExportOptions.ExcludeAttachments;

		    var ex = _exporters.FirstOrDefault(x => x.GetContentType().Equals(ddlTypes.SelectedValue));
            if (ex != null)
                ex.Export(Selection.SelectedItem, options, Response);
		}
	}
}