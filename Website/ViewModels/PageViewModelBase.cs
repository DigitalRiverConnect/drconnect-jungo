using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels
{
    [DataContract]
    public class PageViewModelBase
    {
        // TODO This should be from a resource or actual CMS content - like the Intersection
        private const string DefaultPageTitle = "SportsUS - Free shipping. Free returns. On everything, every day";
        private const string PageTitleSuffix = " - SportsUS";

        public PageViewModelBase()
        {
            Metadata = new PageMetadata();
        }

        private string _title;

        public string PageTitle
        {
            get { return _title ?? DefaultPageTitle; }
        }

        public void SetPageTitle(string title)
        {
            _title = string.IsNullOrEmpty(title) ? DefaultPageTitle : title + PageTitleSuffix;
        }

        private static string CleanMetaField(string strIn)
        {
            if (string.IsNullOrEmpty(strIn)) return string.Empty;
            const string filterPattern = @"[^\w\s\.,$@-]";
            strIn = Regex.Replace(strIn, filterPattern, string.Empty, RegexOptions.Singleline);
            return strIn;
        }

        public string KeyWords { get; set; }
        public string SeoTitleTag { get; set; }

        private string _seoMetaDescription;
        public virtual string SeoMetaDescription
        {
            get { return _seoMetaDescription; }
            set { _seoMetaDescription = CleanMetaField(value); }
        }

        private string _seoMetaKeywords;
        public virtual string SeoMetaKeywords
        {
            get { return _seoMetaKeywords; }
            set { _seoMetaKeywords = CleanMetaField(value); }
        }

        public PageMetadata Metadata { get; set; }
    }

    public class PageMetadata
    {
        public string SiteName { get; set; }

        public string Icon { get; set; }

        public string FacebookAppId { get; set; }

        public string OgImage { get; set; }
        public string OgType { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgSiteName { get; set; }

        public string TwitterCard { get; set; }
        public string TwitterSite { get; set; }
        public string TwitterSiteId { get; set; }
        public string TwitterCreator { get; set; }
        public string TwitterCreatorId { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterTitle { get; set; }
        public string TwitterImageSrc { get; set; }
        public string TwitterImageWidth { get; set; }
        public string TwitterImageHeight { get; set; }
        public string TwitterImage0 { get; set; }
        public string TwitterImage1 { get; set; }
        public string TwitterImage2 { get; set; }
        public string TwitterImage3 { get; set; }
        public string TwitterPlayer { get; set; }
        public string TwitterPlayerWidth { get; set; }
        public string TwitterPlayerHeight { get; set; }
        public string TwitterPlayerStream { get; set; }
        public string TwitterData1 { get; set; }
        public string TwitterLabel1 { get; set; }
        public string TwitterData2 { get; set; }
        public string TwitterLabel2 { get; set; }
        public string TwitterAppNameIphone { get; set; }
        public string TwitterAppIdIphone { get; set; }
        public string TwitterAppUrlIphone { get; set; }
        public string TwitterAppNameIpad { get; set; }
        public string TwitterAppIdIpad { get; set; }
        public string TwitterAppUrlIpad { get; set; }
        public string TwitterAppNameGoogleplay { get; set; }
        public string TwitterAppIdGoogleplay { get; set; }
        public string TwitterAppUrlGoogleplay { get; set; }

        public string ApplicationName { get; set; }
        public string ApplicationSquare150X150Logo { get; set; }
        public string ApplicationSquare70X70Logo { get; set; }
        public string ApplicationWide310X150Logo { get; set; }
        public string ApplicationTileColor { get; set; }
    }
}
