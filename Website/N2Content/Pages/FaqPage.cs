using System.Collections.Generic;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using N2;
using N2.Definitions;
using N2.Details;
using N2.Installation;
using N2.Integrity;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
	[PageDefinition("FAQ",
		Description = "A list of frequently asked questions with answers.",
		SortOrder = 200,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
		IconUrl = "~/Content/img/icons/help.png")]
	[AvailableZone("Questions", "Questions")]
	[RestrictParents(typeof (IStructuralPage))]
    [WithEditableName]
	public class FaqPage : PageModelBase, IStructuralPage
	{
		[EditableChildren("Questions", "Questions", 110 /*, ContainerName = Tabs.Content*/ )]
		public virtual IList<Faq> Questions
		{
			get { return GetChildren<Faq>("Questions"); }
		}
	}
}