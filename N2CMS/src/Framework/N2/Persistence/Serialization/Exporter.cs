using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using N2.Engine;

namespace N2.Persistence.Serialization
{
	/// <summary>
	/// Exports items to a stream.
	/// </summary>
    [Service(typeof(IExporter))]
	public class Exporter : IExporter
	{
		private readonly IItemXmlWriter itemWriter;

	    public Exporter(IItemXmlWriter itemWriter)
		{
		    XmlFormatting = Formatting.Indented;
		    this.itemWriter = itemWriter;
		}

	    public Formatting XmlFormatting { get; set; }

	    public virtual void Export(ContentItem item, ExportOptions options, HttpResponse response)
		{
			response.ContentType = GetContentType();
			response.AppendHeader("Content-Disposition", "attachment;filename=" + GetExportFilename(item));

			using (var output = GetTextWriter(response))
			{
				Export(item, options, output);
				output.Flush();
			}
			response.End();
		}

		public virtual string GetContentType()
		{
			return "application/xml";
		}

		protected virtual TextWriter GetTextWriter(HttpResponse response)
		{
			return response.Output;
		}

		protected virtual string GetExportFilename(ContentItem item)
		{
			return Regex.Replace(item.Title.Replace(' ', '_'), "[^a-zA-Z0-9_-]", "") + ".n2.xml";
		}

		public virtual void Export(ContentItem item, ExportOptions options, TextWriter output)
		{
			var xmlOutput = new XmlTextWriter(output) {Formatting = XmlFormatting};
		    xmlOutput.WriteStartDocument();

			using (var envelope = new ElementWriter("n2", xmlOutput))
			{
				envelope.WriteAttribute("version", GetType().Assembly.GetName().Version.ToString());
				envelope.WriteAttribute("exportVersion", 2);
				envelope.WriteAttribute("exportDate", Utility.CurrentTime());

				itemWriter.Write(item, options, xmlOutput);
			}

			xmlOutput.WriteEndDocument();
			xmlOutput.Flush();
		}
	}
}