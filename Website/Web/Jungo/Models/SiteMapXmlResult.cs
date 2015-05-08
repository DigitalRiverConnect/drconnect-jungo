using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models
{
    public class SiteMapXmlResult : ActionResult
    {
        private readonly IEnumerable<SiteMapEntry> _entries;

        public SiteMapXmlResult(IEnumerable<SiteMapEntry> items)
        {
            _entries = items;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (_entries != null)
            {
                context.HttpContext.Response.ContentType = "text/xml";
                
                //context.HttpContext.TrySetCompressionFilter();                
                //context.HttpContext.Response.SetOutputCache(Utility.CurrentTime().AddDays(1));
                //context.HttpContext.Response.AddCacheDependency(new ContentCacheDependency(Engine.Persister));

                WriteSiteMap(context.HttpContext);
            }
        }

        public virtual string GetBaseUrl(HttpContextBase context)
        {
            return "http://" + context.Request.Url.Authority;
        }

        public void WriteSiteMap(HttpContextBase context)
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };

            using (var writer = XmlWriter.Create(context.Response.Output, settings))
            {
                writer.WriteStartDocument();

                // <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                string baseUrl = GetBaseUrl(context);
                foreach (var item in _entries)
                {
                    WriteItem(writer, baseUrl, item);
                }

                // <urlset>
                writer.WriteEndElement();
            }
        }

        protected virtual void WriteItem(XmlWriter writer, string baseUrl, SiteMapEntry item)
        {
            // <url>
            if (item.Url.StartsWith("http")) return;
            
            writer.WriteStartElement("url");
#if DEBUG2           
            if (!string.IsNullOrEmpty(item.ID))
                writer.WriteElementString("id", item.ID);

            if (!string.IsNullOrEmpty(item.Title))
                writer.WriteElementString("title", item.Title);

            if (!string.IsNullOrEmpty(item.Class))
                writer.WriteElementString("class", item.Class);
#endif   
            writer.WriteElementString("loc", baseUrl + item.Url);
            
            if (item.LastModified.HasValue)
                 writer.WriteElementString("lastmod", item.LastModified.Value.ToString("yyyy-MM-dd")); // Google doesn't like IS0 8601/W3C 
            
            if (item.ChangeFrequency != ChangeFrequencyEnum.Undefined)
                writer.WriteElementString("changefreq", item.ChangeFrequency.ToString().ToLowerInvariant());
            
            writer.WriteElementString("priority", item.Priority.ToString(CultureInfo.InvariantCulture)); 

            // </url>
            writer.WriteEndElement();
        }

        // for testing
        public IEnumerable<SiteMapEntry> Entries
        {
            get { return _entries; }
        }
    }
}