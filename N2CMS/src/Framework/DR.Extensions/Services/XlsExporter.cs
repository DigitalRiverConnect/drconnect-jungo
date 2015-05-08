using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using N2.Definitions;
using N2.Details;
using N2.Engine;
using N2.Models;
using N2.Persistence.Serialization;
using N2.Web.Parsing;

namespace N2.Services
{
    [Service(typeof(IExporter))]
    public class XlsExporter : IExporter
    {
        public class XlsContentItemRecord : XlsRecord
        {
            private readonly ContentItem _contentItem;

            public XlsContentItemRecord(ContentItem contentItem)
            {
                _contentItem = contentItem;
            }

            public override object GetValue(string name)
            {
                object result = Utility.Evaluate(_contentItem, name);
                string str = result as String;

                if (name.Equals("Discriminator") && (!string.IsNullOrEmpty(str)) && str.EndsWith("Proxy"))
                    result = str.Substring(0, str.Length - 5);
                else if (name.Equals("Url") && str.Contains("?"))
                    result = null; // remove links to items - those are confusing for non N2 experts
                else if (result is DetailCollection)
                    result = ((DetailCollection) result).Select(d => d.ToString() + " ").StringJoin();

                return result ?? base.GetValue(name);
            }

            public string Discriminator
            {
                get { return _contentItem.GetType().Name; }
            }
        }

        private readonly IDefinitionManager _definitions;

        public XlsExporter(IDefinitionManager definitions)
        {
            _definitions = definitions;
        }

        public virtual string GetContentType()
        {
            return "application/vnd.ms-excel";
        }

        protected virtual TextWriter GetTextWriter(HttpResponse response)
        {
            return response.Output;
        }

        protected virtual string GetExportFilename(ContentItem item)
        {
            return Regex.Replace(item.Title.Replace(' ', '_'), "[^a-zA-Z0-9_-]", "") + ".n2.xls";
        }

        public void Export(ContentItem item, ExportOptions options, HttpResponse response)
        {
            response.ContentType = GetContentType();
            response.AppendHeader("Content-Disposition", "attachment;filename=" + GetExportFilename(item));

            var items = Find.EnumerateChildren(item).Where(x => !(x is ISystemNode && x.GetType().Name.Contains("Container"))).ToList();
            ExportXlsStream(items, options, response.OutputStream);
            response.End();
        }

        private IEnumerable<string> GetDetailNames(ContentItem item)
        {
            var definition = _definitions.GetDefinition(item);
            return item.Details.Values.Where(detail => definition.Editables.Any(editable => detail.Name == editable.Name)).Select(d => d.Name);
        }

        private void ExportXlsStream(IList<ContentItem> contentItems, ExportOptions options, Stream stream)
        {
            var xls = new XlsModel("Export");
            bool onlydef = (options & ExportOptions.OnlyDefinedDetails) == ExportOptions.OnlyDefinedDetails;

            // add standard columns
            xls.AddColumn("ID");
            xls.AddColumn("Parent.ID");
            xls.AddColumn("AncestralTrail");
            xls.AddColumn("Url");
            xls.AddColumn("PublicUrl"); // for CSS/JS CDN parts
            xls.AddColumn("Name");
            xls.AddColumn("Title");
            xls.AddColumn("Discriminator");
            xls.AddColumn("State");
            xls.AddColumn("Published");
            xls.AddColumn("Updated");
            xls.AddColumn("SavedBy");
            xls.AddColumn("SortOrder");
            xls.AddColumn("Text");
            xls.AddColumn("TemplateKey");
            xls.AddColumn("ViewTemplate");
            xls.AddColumn("ZoneName");

            // get all property names and create columns
            List<string> names = onlydef ? contentItems.SelectMany(GetDetailNames).Distinct().ToList() 
                                         : contentItems.SelectMany(i => i.Details.Keys).Distinct().ToList();

            names.RemoveAll(n => n.StartsWith("Password")); // don't export 
            names.Sort();
            foreach (string name in names)
                xls.AddColumn(name);

            // add all items
            foreach (var item in contentItems)
                xls.Add(new XlsContentItemRecord(item));

            xls.Export(stream);
        }
    }
}