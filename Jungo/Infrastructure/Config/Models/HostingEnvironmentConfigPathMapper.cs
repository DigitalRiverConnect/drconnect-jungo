using System.Web.Hosting;

namespace Jungo.Infrastructure.Config.Models
{
    public class HostingEnvironmentConfigPathMapper : IConfigPathMapper
    {
        public string Map(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}
