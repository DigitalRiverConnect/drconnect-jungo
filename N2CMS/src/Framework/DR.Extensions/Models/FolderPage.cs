using N2.Definitions;
using N2.Details;
using N2.Installation;
using N2.Integrity;

namespace N2.Models
{
	[PageDefinition(title: "Folder", IconUrl = "{IconsUrl}/folder.png", InstallerVisibility = InstallerHint.NeverRootOrStartPage)]
	[RestrictParents(typeof(IRootPage), typeof(FolderPage))]
	[AllowedChildren(typeof(FolderPage), typeof(PartDefinitionPage))] // TODO refine
	[Versionable(AllowVersions.No)]
	[WithEditableTitle]
	public class FolderPage : MetaDataPage, IStructuralPage
	{
		// TODO add	
	}
}