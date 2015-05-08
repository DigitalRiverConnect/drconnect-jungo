using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using N2;
using N2.Definitions;
using N2.Details;
using N2.Engine.Globalization;
using N2.Installation;
using N2.Integrity;
using N2.Web;
using N2.Web.UI;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
	/// <summary>
	/// This is the start page on a site. Separate start pages can respond to 
	/// a domain name and/or form the root of translation.
	/// </summary>
	[PageDefinition("Start Page",
		Description = "The topmost node of a site. This can be placed below an intersection to also represent a language",
		InstallerVisibility=InstallerHint.Default,
		IconUrl = "{IconsUrl}/page_world.png")]
	[RestrictParents(typeof(IRootPage), typeof(LanguageIntersection))]
    [TabContainer(Defaults.Containers.Site, "Site", 1100)]
	// [RecursiveContainer("SiteContainer", 1000, RequiredPermission = Permission.Administer)]
	// [FieldSetContainer(Defaults.Containers.Site, "Site", 1000, ContainerName = "SiteContainer")]
    public class StartPage : PageModelBase, IStartPage, ILanguage, ISitesSource, IRedirect 
	{
		#region ILanguage Members

		public string FlagUrl
		{
			get
			{
				if (string.IsNullOrEmpty(LanguageCode))
					return "";

				var parts = LanguageCode.Split('-');
				return N2.Web.Url.ResolveTokens(string.Format("~/N2/Resources/Img/Flags/{0}.png", parts[parts.Length - 1].ToLower()));
			}
		}

        public override string IconUrl
        {
            get
            {
                var flagUrl = FlagUrl;
                return string.IsNullOrEmpty(flagUrl) ? base.IconUrl : flagUrl;
            }
        }

		[EditableLanguagesDropDown(Title="Languages",ContainerName=Defaults.Containers.Site)]
        public string LanguageCode
        {
            get { return ((string)GetDetail("LanguageCode") ?? ""); }
            set { SetDetail("LanguageCode", value, ""); }
        }

		public string LanguageTitle
		{
			get
			{
			    return string.IsNullOrEmpty(LanguageCode) ? "" : new CultureInfo(LanguageCode).DisplayName;
			}
		}

		#endregion

		[EditableText(Title = "Global Commerce Site ID",  
                      ContainerName = Defaults.Containers.Site,
					  HelpTitle = "Global Commerce Site ID")]
		public virtual string SiteID
        {
            get { return ((string)GetDetail("SiteID") ?? ""); }
            set { SetDetail("SiteID", value, ""); }
        }

	    [EditableCheckBox(Title = "Is Opt-in checked for new shopper", SortOrder=11000,
	        ContainerName = Defaults.Containers.Site,
	        HelpText = "When creating a new shopper, determines whether the email opt-in flag is set or not.")]
	    public virtual bool SendEmailDefault
	    {
            get { return (bool)(GetDetail("SendEmailDefault") ?? true); }
	        set { SetDetail("SendEmailDefault", value, true); }
	    }

        [EditableText(Title = "Promotion ID", SortOrder = 11100,
            ContainerName = Defaults.Containers.Site,
            HelpText = "Promotion ID for this cross sell promotion.", Required = true, DefaultValue = "")]
        public virtual string PromotionId { get; set; }

        // Was intended to be used for page title suffixes, but was left unused in 14.04
        //[EditableMetaTag(Title = "Site Name", SortOrder = 200,
        //    ContainerName = Defaults.Containers.Metadata,
        //    HelpText = "Site name used for meta tags and page title suffixes")]
        //public virtual string SiteName { get; set; }

        [EditableMetaTag(Title = "Facebook App ID", SortOrder = 205,
            ContainerName = Defaults.Containers.Metadata,
            HelpText = "Facebook App ID for the store")]
        public virtual string FacebookAppId { get; set; }

        [EditableMetaTag(Title = "Twitter Site", SortOrder = 210,
            ContainerName = Defaults.Containers.Metadata,
            HelpText = "Twitter Site for the store")]
        public virtual string TwitterSite { get; set; }

        [EditableMetaTag(Title = "OpenGraph - Site Name", SortOrder = 400,
            ContainerName = Defaults.Containers.Metadata,
            HelpText = "OpenGraph Site Name for the store")]
        public virtual string OgSiteName { get; set; }

        [EditableMetaTag(Title = "application-name", SortOrder = 500,
            ContainerName = Defaults.Containers.Metadata,
            HelpText = "Application name for pinned site metadata")]
        public virtual string ApplicationName { get; set; }

	    #region ISitesSource Members

		[EditableText(Title = "Site collection host name (DNS)",
			ContainerName = Defaults.Containers.Site,
			HelpTitle = "Sets a shared host name for all languages on a site. The web server must be configured to accept this host name for this to work.")]
		public virtual string HostName { get; set; }

		public IEnumerable<Site> GetSites()
		{
			if (!string.IsNullOrEmpty(HostName))
				yield return new Site(Find.EnumerateParents(this, null, true).Last().ID, ID, HostName) { Wildcards = true };
		}

		#endregion

        #region IRedirect Members

        public string RedirectUrl
        {
            get { return Children.OfType<LanguageRoot>().Select(sp => sp.Url).FirstOrDefault() ?? Url; }
        }

        public ContentItem RedirectTo
        {
            get { return Children.OfType<LanguageRoot>().FirstOrDefault(); }
        }

        #endregion
	}
}