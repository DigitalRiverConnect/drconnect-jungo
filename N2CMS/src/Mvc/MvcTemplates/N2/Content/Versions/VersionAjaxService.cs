using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
#if XML_DIFF
using Microsoft.XmlDiffPatch;
#endif
using N2.Edit;
using N2.Edit.Versioning;
using N2.Engine;
using N2.Security;
using N2.Web;

namespace N2.Management.Content.Navigation
{
	[Service(typeof(IAjaxService))]
	public class VersionAjaxService : IAjaxService
	{
        private readonly Navigator _navigator;
		private readonly ContentVersionRepository _versionRepository;
        private readonly ISecurityManager _security;
        private readonly IWebContext _webContext;

        public VersionAjaxService(Navigator navigator, ContentVersionRepository versionRepository, IWebContext webContext, ISecurityManager security)
        {
            _navigator = navigator;
			_versionRepository = versionRepository;
            _security = security;
            _webContext = webContext;
        }

		#region IAjaxService Members

		public string Name
		{
			get { return "version"; }
		}

		public bool RequiresEditAccess
		{
			get { return true; }
		}

		/// <summary>Gets whether request's HTTP method is valid for this service.</summary>
		public bool IsValidHttpMethod(string httpMethod)
		{
		    return httpMethod == "GET";
		}

        /// <summary>
        /// Get the XML for a version of an item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="versionQuery"></param>
        /// <returns></returns>
        private string GetXml(ContentItem item, string versionQuery)
        {
            int index;
            int.TryParse(versionQuery, out index);
            var ver = _versionRepository.GetVersion(item, index);
            return ver != null ? ver.VersionDataXml : _versionRepository.Serialize(item);
        }

		public void Handle(HttpContextBase context)
		{
		    var found = false;
			string path = context.Request["path"];
            string versionQuery = context.Request.QueryString[PathData.VersionIndexQueryKey];
            //string versionKey = context.Request.QueryString[PathData.VersionKeyQueryKey];

            var item = _navigator.Navigate(path);
            
            // enforce security
            if (!_security.IsAuthorized(item, _webContext.User))
                throw new UnauthorizedAccessException();

		    if (item != null)
		    {
		        if (string.IsNullOrEmpty(versionQuery))
		        {
		            var versions = _versionRepository.GetVersions(item);
		            var children = versions.Select(ToJson).ToArray();
		            context.Response.Write("{\"path\":\"" + Encode(item.Path) + "\", \"children\":[" +
		                                   string.Join(", ", children) + "]}");
		            context.Response.ContentType = "application/json";
                    found = true;
		        }
		        else
		        {
                    var xml = GetXml(item, versionQuery);
                    if (xml != null)
		            {
#if XML_DIFF
                        string versionDiff = context.Request.QueryString["versionDiff"];
                        var xml2 = string.IsNullOrEmpty(versionDiff) ? null : GetXml(item, versionDiff);
                        if (xml2 != null)
                        {
                            xml = GetXmlDiff(xml, xml2);
                        }
#endif
                        context.Response.ContentType = "application/xml";
                        context.Response.Write(xml);
                        found = true;
		            }
		        }
		    }
		    
            if (!found)
		    {
                context.Response.ContentType = "text/plain";
                context.Response.Write("version not found");
		        context.Response.StatusCode = 404;
		    }
		}

#if XML_DIFF
        /// <summary>
        /// Create a diff between 2 xmls
        /// http://msdn.microsoft.com/en-us/library/aa302294.aspx
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xml2"></param>
        /// <returns></returns>
        private static string GetXmlDiff(string xml, string xml2)
        {
            using (var sw = new StringWriter())
            {
                using (XmlWriter diffgramWriter = new XmlTextWriter(sw))
                {
                    XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder |
                                                  XmlDiffOptions.IgnoreNamespaces |
                                                  XmlDiffOptions.IgnorePrefixes);

                    XmlTextReader reader1 = new XmlTextReader(new StringReader(xml));
                    XmlTextReader reader2 = new XmlTextReader(new StringReader(xml2));

                    bool bIdentical = xmldiff.Compare(reader1, reader2, diffgramWriter);
                }
                return sw.ToString();
            }
        }
#endif

        private static string ToJson(ContentVersion c)
        {
            return string.Format("{{\"index\":{0}, " +
                                 "\"state\":\"{1}\", " +
                                 "\"saved\":\"{4}\", " +
                                 "\"savedBy\":\"{3}\", " +
                                 "\"title\":\"{2}\"}}",
                c.VersionIndex, c.State.ToString(), Encode(c.Title),
                c.SavedBy, c.Saved.ToShortDateString() + " " + c.Saved.ToShortTimeString());
        }

		private static string Encode(string text)
		{
			return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
		}

		#endregion
	}
}