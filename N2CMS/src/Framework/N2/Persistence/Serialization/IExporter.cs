using System.Web;

namespace N2.Persistence.Serialization
{
    public interface IExporter
    {
        string GetContentType();
        void Export(ContentItem item, ExportOptions options, HttpResponse response);
    }
}