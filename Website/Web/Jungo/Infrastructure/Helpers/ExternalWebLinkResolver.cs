using System;
using Jungo.Infrastructure.Config;
using N2.Edit.FileSystem;
using N2.Interfaces;
using N2.Resources;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers
{
    public class ExternalWebLinkResolver : IExternalWebLinkResolver
    {
        #region Private Cloudlink Config

        private string _publicUrlConfig; // starts out null, meaning haven't gotten config value yet

        private string PublicUrlConfig
        {
            get
            {
                if (_publicUrlConfig != null) return _publicUrlConfig;

                _publicUrlConfig = ConfigLoader.Get<ExternalWebLinkConfig>().PublicUrl;
                if (!string.IsNullOrWhiteSpace(_publicUrlConfig)) return _publicUrlConfig;

                // get from filesystem root if possible, otherwise assume local
                var webAccessible = N2.Context.Current.Container.Resolve<IFileSystem>() as IWebAccessible;
                _publicUrlConfig = webAccessible != null ? webAccessible.GetPublicURL(null) : string.Empty;

                return _publicUrlConfig;
            }
        }

        #endregion

        #region IExternalWebLinkResolver Members

        public string GetPublicUrl(string filePath)
        {
            var url = ToPublicUrl(filePath, PublicUrlConfig);
            return url;
        }
        
        #endregion

        // public for testing
        public static string ToPublicUrl(string filePath, string publicUrlConfig)
        {
            if (string.IsNullOrEmpty(filePath) || filePath.StartsWith("/"))
                return filePath;

            if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
                return RewriteUri(filePath, publicUrlConfig);

            if (!string.IsNullOrEmpty(publicUrlConfig))
                return FormatPublicUrl(null, publicUrlConfig, filePath);
            
            var publicUrl = Url.ToAbsolute(filePath);
            if (!publicUrl.StartsWith("/"))
                return "/" + publicUrl;
            
            return publicUrl;
        }

        // public for testing
        public static string FormatPublicUrl(string scheme, string configPublicUrl, string filePath)
        {
            // prepare path
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;
            if (filePath.StartsWith("~"))
                filePath = filePath.Remove(0, 1);
            if (configPublicUrl.EndsWith("/") == filePath.StartsWith("/"))
                filePath = configPublicUrl.EndsWith("/") ? filePath.Remove(0, 1) : "/" + filePath;

            if (!string.IsNullOrEmpty(scheme))
            {
                // fall back to supplied scheme if missing from configured
                return configPublicUrl.IndexOf("://", StringComparison.Ordinal) < 0
                           ? scheme + "://" + configPublicUrl.TrimStart('/') + filePath
                           : configPublicUrl + filePath;
            }

            // no scheme
            if (configPublicUrl.StartsWith("//"))
                return configPublicUrl + filePath;
            if (configPublicUrl.StartsWith("http"))
                return UriWithoutProtocol(configPublicUrl + filePath);

            return "//" + configPublicUrl.TrimStart('/') + filePath;
        }

        // public for testing
        public static string UriWithoutProtocol(string url)
        {
            var uri = new Uri(url);
            return "//" + uri.Host + uri.PathAndQuery;
        }

        private static string RewriteUri(string url, string configPublicUrl)
        {
            var uri = new Uri(url);
            if (uri.Host.EndsWith("blob.core.windows.net"))
            {
                // handles the special case for CS/JSS which are using a different container
                // in order to leave the configuration as is - we tweak the strings accordingly...]
                var cpu = configPublicUrl.StartsWith("//") || configPublicUrl.StartsWith("http") ? configPublicUrl : "//" + configPublicUrl;
                var configUri = new Uri(cpu);
                return "//" + configUri.Host + uri.PathAndQuery;
            }

            return "//" + uri.Host + uri.PathAndQuery;
        }
    }

    [Serializable]
    public class ExternalWebLinkConfig
    {
        public string PublicUrl { get; set; }
        public string ProductImageUrl { get; set; }
    }
}
