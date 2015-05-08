//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
//

using System.Linq;
using N2;
using N2.Definitions;
using N2.Installation;
using N2.Integrity;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
	/// <summary>
	/// Redirects to the child start page that matches the user agent's language.
	/// </summary>
	[PageDefinition("Intersection", 
		Description = "Redirects to the child start page that matches the user agent's language and locale.", 
		InstallerVisibility=InstallerHint.PreferredStartPage,
		IconUrl="~/favicon.ico")]
	[RestrictParents(typeof(IRootPage))]
    [Versionable(AllowVersions.No)]
    [Throwable(AllowInTrash.No)]
    // [RecursiveContainer("SiteContainer", 1000, RequiredPermission = Permission.Administer)]
	// [FieldSetContainer(Defaults.Containers.Site, "Site", 1000, ContainerName = "SiteContainer")]
	public class LanguageIntersection : PageModelBase, IRedirect, IStartPage 
	{
        #region IRedirect Members

		public string RedirectUrl
		{
			get { return Children.OfType<StartPage>().Select(sp => sp.Url).FirstOrDefault() ?? Url; }
		}

		public ContentItem RedirectTo
		{
			get { return Children.OfType<StartPage>().FirstOrDefault(); }
		}

		#endregion
	}
}