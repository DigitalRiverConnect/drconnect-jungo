using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Hosting;
using N2.Engine;
using N2.Models;
using N2.Persistence;
using N2.Plugin;
using N2.Web;
using N2.Web.UI;

namespace N2.Services
{
    [Service]
    public class ContentVirtualPathProvider : VirtualPathProvider, IAutoStart
    {
        private Logger<ContentVirtualPathProvider> logger;
        private readonly IPersister _persister;
        private readonly ContentPartTemplateProvider _partTemplateProvider;
	    private const string BaseDirectory = "/Views/ContentParts/PartTemplates/"; // TODO get rid of magic value - must match Controller

        public ContentVirtualPathProvider(IPersister persister, ContentPartTemplateProvider partTemplateProvider)
        {
            _persister = persister;
            _partTemplateProvider = partTemplateProvider;
        }

        #region IAutoStart Members

        public void Start()
        {
            if (HostingEnvironment.IsHosted)
                HostingEnvironment.RegisterVirtualPathProvider(this);
        }

        public void Stop()
        {
        }

        #endregion

        #region Internal

        //private int InternalGetIdByPath(string virtualPath)
        //{
        //    if (!virtualPath.StartsWith(BaseDirectory))
        //        throw new ArgumentException("virtualPath not for content item");

        //    var file = Path.GetFileNameWithoutExtension(virtualPath);
        //    return _partTemplateProvider.GetIdForPartName(file);   
        //}

        private bool InternalFileExists(string virtualPath)
        {
            return InternalGetFileData(virtualPath, true) != null;
        }

        private static byte[] dummy = {0};

		// assume file name is numerical ID of ContentItem
		private byte[] InternalGetFileData(string virtualPath, bool existsCheck = false)
		{
            if (!virtualPath.StartsWith(BaseDirectory))
                return null;

            var file = Path.GetFileNameWithoutExtension(virtualPath);
		    int id = _partTemplateProvider.GetIdForPartName(file);
		    if (id == 0)
		    {
		        if (!int.TryParse(file, out id))
		            return null;
		    }

			var item = _persister.Get(id) as PartDefinitionPage;
			if (item == null) return null;

            if (existsCheck) return dummy;

            var data = item.Template ?? "";
#if DEBUG
            logger.DebugFormat("Get ContentVPP ID: {0} at {1} bytes={2}", item, virtualPath, data.Length);
#endif
		    if (!string.IsNullOrEmpty(data))
		    {
		        return Encoding.UTF8.GetBytes(data);
		    }
		    return null;
		}

		#endregion

		public override bool FileExists(string virtualPath)
		{
			return InternalFileExists(virtualPath) || Previous.FileExists(virtualPath);
		}

		public override VirtualFile GetFile(string virtualPath)
		{
			return InternalFileExists(virtualPath) 
				? new PartDefinitionVirtualFile(virtualPath, InternalGetFileData(virtualPath))
				: Previous.GetFile(virtualPath);
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
            if (virtualPath == null || !virtualPath.StartsWith(BaseDirectory)) 
                return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

            var filesNotBelongingToSelf = virtualPathDependencies.OfType<string>().Select(Url.ToRelative).Where(f => !InternalFileExists(f.TrimStart('~'))).ToList();

			return filesNotBelongingToSelf.Any()
				       ? Previous.GetCacheDependency(virtualPath, filesNotBelongingToSelf, utcStart)
                       : new ContentCacheDependency(_persister);
//                       : new ContentCacheDependency(_workflow, InternalGetIdByPath(virtualPath)); // TODO new CacheDependency(...);
		}

		//public override string GetCacheKey(string virtualPath)
		//{
		//    return null;
		//}

		//public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		//{
		//    return virtualPath;
		//}

		public class PartDefinitionVirtualFile : VirtualFile
		{
		    private readonly string _virtualPath;
		    private readonly byte[] _data;

			public PartDefinitionVirtualFile(string virtualPath, byte[] data) : base(virtualPath)
			{
			    _virtualPath = virtualPath;
			    _data = data;
			}

		    public override Stream Open()
			{
				return new MemoryStream(_data);
			}
		}
	}
}