using N2.Edit;
using N2.Engine;
using N2.Models;
using N2.Web;

namespace N2.Services
{
	/// <summary>
	/// Needed to override tree navigation
	/// </summary>
	[Adapts(typeof(MetaDataPage))]
	public class MetaDataPageNodeAdapter : NodeAdapter
	{
		public override string GetPreviewUrl(ContentItem item)
		{
			Url url = Url.Parse("{ManagementUrl}/Empty.aspx");

			if (item is PartDefinitionPage)
				url = Url.Parse("{ManagementUrl}/Content/Edit.aspx").AppendQuery(SelectionUtility.SelectedQueryKey, item.Path);

			return url.ResolveTokens();
		}
	}
}