using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using Microsoft.WindowsAzure.ServiceRuntime;
using N2;
using N2.Azure;
using N2.Azure.Replication;
using N2.Configuration;
using N2.Definitions;
using N2.Persistence;
using N2.Persistence.Serialization;
using N2.Persistence.Xml;
using N2.Web;
using Newtonsoft.Json;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    public class DiagnosticController : Controller
    {
        //
        // GET: /Diagnostic/
        private readonly IContentItemRepository _repository;
        private readonly IHost _host;
        private readonly IDefinitionManager _definitions;
        private readonly ILinkGenerator _linkGenerator;
        private readonly IUrlParser _parser;
        private readonly IFlushable _flushable;
        private readonly object _syncLock = new object();
        private readonly string _tablePrefix;
        private readonly IReplicationStorage _repstore;

        private static ReplicationForceWriteLockManager _forceWriteLockManager;
        private static ReplicationWriteLockManager _writeLockManager;

        public DiagnosticController(IContentItemRepository repository, IHost host, IDefinitionManager definitions,
            ILinkGenerator linkGenerator, IUrlParser parser, DatabaseSection config, IFlushable flushable, IReplicationStorage repstore, 
            IFileSystemFactory fileSystemFactory)
        {
            _repository = repository;
            _host = host;
            _definitions = definitions;
            _linkGenerator = linkGenerator;
            _parser = parser;
            _flushable = flushable;
            _tablePrefix = config.TablePrefix;

            _repstore = repstore;

            if (_forceWriteLockManager != null) return;

            // Create Force Write Lock Manager
            var storageConfig = (FileSystemNamespace) Enum.Parse(typeof (FileSystemNamespace),
                ConfigurationManager.AppSettings["AzureReplicationStorageContainerName"] ??
                "ReplicationStorageDebug");

            var fileSystem = fileSystemFactory.Create(storageConfig);
            _forceWriteLockManager = new ReplicationForceWriteLockManager(fileSystem);
            _writeLockManager = new ReplicationWriteLockManager(fileSystem);
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Index()
        {
            var text = new StringBuilder();
            text.AppendLine("STATUS");
            text.AppendLine(GetCodeVersion());

            if (RoleEnvironment.IsAvailable)
            {
                text.AppendLine("DeploymentId: " + RoleEnvironment.DeploymentId);
                var roleInstance = RoleEnvironment.CurrentRoleInstance;
                foreach (RoleInstanceEndpoint instanceEndpoint in roleInstance.InstanceEndpoints.Values)
                {
                    text.AppendLine("Instance endpoint address and port: " + instanceEndpoint.IPEndpoint);
                    text.AppendLine("Protocol for the endpoint: " + instanceEndpoint.Protocol);
                }
            }
#if DEBUG
            GC.Collect();
            GC.WaitForFullGCComplete();
#endif
            //text.AppendLine("ContentItem Live Instances: " + ContentItem.InstanceCount);

            var root = _repository.Get(_host.CurrentSite.RootItemID);
            text.AppendLine("ContentItem Root:           " + root);
            text.AppendLine("ContentItems In Database:");

            var discriminators = _repository.FindDescendantDiscriminators(root).ToDictionary(d => d.Discriminator, d => d.Count);
            int total = 0;
            foreach (var definition in _definitions.GetDefinitions().OrderBy(d => d.Discriminator))
            {
                int numberOfItems;
                discriminators.TryGetValue(definition.Discriminator, out numberOfItems);
                definition.NumberOfItems = numberOfItems;
                if (numberOfItems > 0)
                    text.AppendLine(string.Format("-  {0:D6}: {1}", numberOfItems, definition.Discriminator));
                total += numberOfItems;
            }
            text.AppendLine("= Total ContentItems: " + total);
            text.AppendLine();
            text.AppendLine("ViewTemplate Statistics:");


            var xr = _repository as XmlContentItemRepository;
            if (xr != null)
            {
                text.AppendLine();
                text.AppendLine("Items in Cached Repository: " + xr.AllContentItems.Count());
                foreach (var item in xr.AllContentItems)
                {
                    text.AppendLine(item + " " + item.Path);
                }
            }

            return Content(text.ToString(), "text/plain");
        }

        public class LinkModel
        {
            public string Caption;
            public string Url;
            public string Target;
            public string Source;
            public string Clazz;
            public string Error;

            public bool IsExternal()
            {
                return Url != null && !Url.StartsWith("http://") && !Url.StartsWith("https://") && !Url.StartsWith("//");
            }

            public bool IsInternal()
            {
                return !string.IsNullOrEmpty(Url) && !IsExternal();
            }

            public override string ToString()
            {
                return string.Format("{4} {0} '{1}': {2} -> {3}", Source, Caption, Url, Target, Clazz);
            }
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Tree(bool? all)
        {
            var root = _repository.Get(_host.CurrentSite.RootItemID);

            var model = new DiagnosticTree
            {
                ShowAll = all ?? false,
                Root = new DiagnosticTreeNode()
            };

            model.Root = BuildTree(root);

            return View(model);
        }

        private static DiagnosticTreeNode BuildTree(ContentItem item)
        {
            var treeNode = new DiagnosticTreeNode
            {
                Name = item.Name,
                Id = item.ID,
                Type = item.GetContentType().Name,
                IsPage = item.IsPage,
                Children = new List<DiagnosticTreeNode>()
            };

            treeNode.Children.AddRange(item.Children.Select(BuildTree));

            return treeNode;
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Links()
        {
            var prefix = Request["prefix"] ?? "/start/msusa/store/";
            var kind = Request["kind"] ?? "";
            var model = new List<LinkModel>();

            var xr = _repository as XmlContentItemRepository;
            if (xr != null)
            {
                IEnumerable<ListofLinksItem> list = xr.AllContentItems.OfType<ListofLinksItem>();
                foreach (var link in list)
                {
                    if (link.Path.StartsWith(prefix))
                        continue;

                    if (link.SuppressLinks)
                        continue;

                    var lm = new LinkModel
                    {
                        Source = link.Path,
                        Caption = string.IsNullOrEmpty(link.LinkText) ? link.Title : link.LinkText,
                        Url = link.GetUrl(_linkGenerator),
                        Clazz = link.GetType().Name
                    };
                    try
                    {
                        if (lm.IsExternal())
                        {   // resolve internal target
                            var data = _parser.FindPath(lm.Url);
                            if (data != null)
                                lm.Target = data.CurrentPage.Path;
                        }
                    }
                    catch (Exception ex)
                    {
                        lm.Target = "***";
                        lm.Error = ex.Message;
                    }

                    if (lm.IsExternal()) model.Add(lm);
                }
            }

            // render
            var text = new StringBuilder();
            text.AppendLine("LINKS");
            foreach (var linkModel in model)
            {
                text.AppendLine(linkModel.ToString());
            }

            return Content(text.ToString(), "text/plain");
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Files()
        {
            var text = new StringBuilder("<html><head></head><body><h1>Files</h1>");

            var xr = _repository as XmlContentItemRepository;
            if (xr != null)
            {
                var files = Directory.GetFileSystemEntries(xr.DataDirectoryPhysical, "*");
                foreach (var s in files)
                {
                    text.AppendFormat("<div><a href=\"{0}\">{1}</a><div>", Url.RouteUrl(new { controller = "Diagnostic", action = "File", id = Path.GetFileName(s) }), Path.GetFileName(s));
                }
            }

            text.Append("</body></html>");

            return Content(text.ToString(), "text/html");
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult File(string id)
        {
            var xr = _repository as XmlContentItemRepository;
            var f = System.IO.File.OpenText(Path.Combine(xr.DataDirectoryPhysical, id));
            var xml = f.ReadToEnd();
            return Content(xml, "text/xml");
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult RepStoreFiles()
        {
            var text = new StringBuilder("<html><head></head><body><h1>Files</h1>");

            var items = _repstore.GetItems();
            text.Append("<div>RepStore Available</div>");
            foreach (var item in items)
            {
                text.AppendFormat("<div>File ID: {0}<div>", item.ID);
            }

            text.Append("</body></html>");

            return Content(text.ToString(), "text/html");
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Cache()
        {
            var text = new StringBuilder();
            text.AppendLine("CACHE");

            var urlParser = Context.Current.Resolve<IUrlParser>() as CachingUrlParserDecorator;
            if (urlParser != null)
            {
                text.AppendLine("PATH DATA CACHE (used to route requests)");
                lock (_syncLock)
                {
                    var obj = HttpRuntime.Cache.Get(_tablePrefix + CachingUrlParserDecorator._n2Pathdatacache);
                    var cachedPathData = obj as Dictionary<string, PathData>;
                    if (cachedPathData != null)
                        foreach (var de in cachedPathData)
                        {
                            text.AppendLine(de.Key + " -> " + de.Value);
                        }
                }

                foreach (var cacheKey in CachingUrlParserDecorator.allCacheKeys)
                {
                    text.AppendLine("");
                    text.AppendLine("BUILD URL CACHE (used to build item URLs) " + cacheKey);
                    lock (_syncLock)
                    {
                        var obj = HttpRuntime.Cache.Get(cacheKey);
                        var itemToUrlCache = obj as Dictionary<int, string>;
                        if (itemToUrlCache != null)
                            foreach (var de in itemToUrlCache)
                            {
                                text.AppendLine(de.Key + " -> " + de.Value);
                            }
                    }
                }

                if (Request["flush"] != null && _flushable != null)
                {
                    _flushable.Flush();
                    text.AppendLine("FLUSHED after listing contents via " + _flushable.GetType().Name);
                }
            }

            return Content(text.ToString(), "text/plain");
        }


        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Item(int id)
        {
            var item = _repository.Get(id);
            if (item != null)
                try
                {
                    var itemXmlWriter = new ItemXmlWriter(Context.Current.Definitions, null, null);
                    var exporter = new Exporter(itemXmlWriter);
                    var ms = new MemoryStream();
                    var tw = new StreamWriter(ms);
                    {
                        exporter.Export(item, ExportOptions.ExcludeAttachments | ExportOptions.ExcludePages, tw);
                    }
                    ms.Position = 0;
                    return new FileStreamResult(ms, "application/xml");
                }
                catch (Exception ex)
                {
                    return Content(ex.Message, "text/plain");
                }

            return new EmptyResult();
        }

        [HttpGet]
#if !DEBUG
        [ClientCertAuthorization(Controller = "Home", Action = "Index")]
#endif
        public ActionResult Lock(int? duration)
        {
            var isLocked = _forceWriteLockManager.Lock(duration);

            var lockSpan = new TimeSpan(0, 0, 0, 0,
                duration ?? int.Parse(ReplicationLockManagerBase.DefaultTimerInterval));

            return View(new DiagnosticLock
            {
                IsLocked = isLocked,
                LockDuration = lockSpan
            });
        }

        public ActionResult LockNormal(int? duration)
        {
            var isLocked = _writeLockManager.Lock(duration);

            var lockSpan = new TimeSpan(0, 0, 0, 0,
                duration ?? int.Parse(ReplicationLockManagerBase.DefaultTimerInterval));

            return View("Lock", new DiagnosticLock
            {
                IsLocked = isLocked,
                LockDuration = lockSpan
            });
        }

        public ActionResult Unlock()
        {
            _forceWriteLockManager.Unlock();
            
            return View(new DiagnosticLock
            {
                IsLocked = false,
                LockDuration = new TimeSpan()
            });
        }

#if DEBUG
        [HttpGet]
        public ActionResult ViewTester()
        {
            var views = GetViewFileNames(Request.PhysicalApplicationPath);
            var model = new DiagnosticViewTesterModel
            {
                RootFolder = BuildViewTree(views)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult ViewTester(DiagnosticViewTesterModel model)
        {
            var json = string.Empty;
            if (!string.IsNullOrEmpty(model.Json))
            {
                json = model.Json;
            }
            else if (model.JsonFile != null && model.JsonFile.ContentLength > 0)
            {
                var bytes = new byte[model.JsonFile.ContentLength];
                model.JsonFile.InputStream.Read(bytes, 0, model.JsonFile.ContentLength);
                json = Encoding.UTF8.GetString(bytes);
            }
            if (string.IsNullOrEmpty(model.SelectedViewName))
            {
                model.ErrorMessage = "No View selected";
                return View(model);
            }
            var modelName = ParseViewForModelName(model.SelectedViewName, Request.PhysicalApplicationPath);
            var typ = StringToLoadedType(modelName);
            object mod;
            try
            {
                mod = typ == null || string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject(json, typ);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
            ViewBag.Title = string.Format("view tester - {0}", model.SelectedViewName);
            ForceViewLocation(ViewEngineCollection, model.SelectedViewName);
            return View(model.SelectedViewName, mod);
        }
#endif

        /// <summary>
        /// This simply looks for the file version.txt and reads a single line from it
        /// to be returned to the caller.  
        /// </summary>
        /// <returns>The current version of the code that's deployed</returns>
        private bool TryGetCodeVersion(out string versionInfo, out DateTime lastModified)
        {
            const string versionFile = "~/revision.txt";
            try
            {
                var fileInfo = new FileInfo(Server.MapPath(versionFile));
                lastModified = fileInfo.LastWriteTime;
                using (var f = fileInfo.OpenText())
                {
                    versionInfo = f.ReadLine();
                }
                return true;
            }
            catch (Exception ex)
            {
                lastModified = default(DateTime);
                versionInfo = "revision.txt file not found " + ex.Message;
                return false;
            }
        }

        private string GetCodeVersion()
        {
            string versionInfo;
            DateTime lastModified;

            return TryGetCodeVersion(out versionInfo, out lastModified) ? versionInfo : "?";
        }

#if DEBUG
        private static string ParseViewForModelName(string viewFileName, string appPath)
        {
            if (appPath.EndsWith(@"\")) appPath = appPath.Remove(appPath.Length - 1);
            if (viewFileName.StartsWith("~"))
                viewFileName = viewFileName.Remove(0, 1).Insert(0, appPath);
            using (var f = System.IO.File.OpenText(viewFileName))
            {
                var line = f.ReadLine();
                var lineCount = 0;
                while (line != null && lineCount < 20)
                {
                    if (line.StartsWith("@model "))
                        return line.Substring(7);
                    line = f.ReadLine();
                    lineCount++;
                }
            }
            return string.Empty;
        }

        private static Type StringToLoadedType(string typeName)
        {
            // look for full name
            var refedAsms = new HashSet<string>();
            foreach (var loadedAsm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in loadedAsm.GetTypes().Where(t => t.FullName == typeName))
                    return t;
                foreach (var ra in loadedAsm.GetReferencedAssemblies().Where(ra => !refedAsms.Contains(ra.FullName) && typeName.StartsWith(ra.FullName)))
                {
                    refedAsms.Add(ra.FullName);
                    var refedAsm = Assembly.Load(ra);
                    foreach (var t in refedAsm.GetTypes().Where(t => t.FullName == typeName))
                        return t;
                }
            }
            // look for short name
            refedAsms.Clear();
            var types = new List<Type>();
            var dottedTypeName = "." + typeName;
            foreach (var loadedAsm in AppDomain.CurrentDomain.GetAssemblies())
            {
                types.AddRange(loadedAsm.GetTypes().Where(t => t.FullName.EndsWith(dottedTypeName)));
                foreach (var ra in loadedAsm.GetReferencedAssemblies().Where(ra => !refedAsms.Contains(ra.FullName)))
                {
                    refedAsms.Add(ra.FullName);
                    var refedAsm = Assembly.Load(ra);
                    types.AddRange(refedAsm.GetTypes().Where(t => t.FullName.EndsWith(dottedTypeName)));
                }
            }
            return types.Count == 1 ? types[0] : null;
        }

        private static IEnumerable<string> GetViewFileNames(string appPath)
        {
            if (!appPath.EndsWith(@"\"))
                appPath += @"\";
            appPath += @"Views\";
            var files = Directory.GetFiles(appPath, "*.cshtml", SearchOption.AllDirectories);
            return files.Select(file => "~" + file.Substring(appPath.Length - 7)).ToArray();
        }

        private static DiagnosticViewTesterTree BuildViewTree(IEnumerable<string> viewFileNames)
        {
            var root = new DiagnosticViewTesterTree("Views");
            foreach (var viewFileName in viewFileNames)
            {
                var parts = viewFileName.Substring(8).Split('\\');
                var folderNode = root;
                for (var idx = 0; idx < parts.Length - 1; idx++)
                {
                    var tempNode = folderNode.ContainedFolders.FirstOrDefault(f => f.FolderName == parts[idx]);
                    if (tempNode == null)
                    {
                        tempNode = new DiagnosticViewTesterTree(parts[idx]);
                        folderNode.ContainedFolders.Add(tempNode);
                    }
                    folderNode = tempNode;
                }
                folderNode.ContainedFileNames.Add(viewFileName + "," + parts[parts.Length - 1]);
            }
            return root;
        }

        private static void ForceViewLocation(ViewEngineCollection viewEngineCollection, string viewLocation)
        {
            var forcedLocViewEngine =
                viewEngineCollection.OfType<ForcedLocationViewEngine>().Select(ve => ve).FirstOrDefault();
            if (forcedLocViewEngine == null)
            {
                forcedLocViewEngine = new ForcedLocationViewEngine();
                viewEngineCollection.Add(forcedLocViewEngine);
            }
            forcedLocViewEngine.SetLocation(viewLocation);
        }

        private class ForcedLocationViewEngine : RazorViewEngine
        {
            public void SetLocation(string viewLocation)
            {
                ViewLocationFormats = new[] { viewLocation };
            }
        }
#endif
    }

#if DEBUG
    public class DiagnosticViewTesterViewInfo
    {
        public string ViewInfo {
            get { return ViewPath + "," + ViewName; }
        }
        public string ViewName { get; set; }
        public string ViewPath { get; set; }
    }
    public class DiagnosticViewTesterModel
    {
        public string SelectedViewName { get; set; }
        public DiagnosticViewTesterTree RootFolder { get; set; }
        public string Json { get; set; }
        public HttpPostedFileBase JsonFile { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class DiagnosticViewTesterTree
    {
        public DiagnosticViewTesterTree(string folderName)
        {
            FolderName = folderName;
            ContainedFileNames = new List<string>();
            ContainedFolders = new List<DiagnosticViewTesterTree>();
        }
        public string FolderName { get; private set; }
        public List<DiagnosticViewTesterTree> ContainedFolders { get; private set; }
        public List<string> ContainedFileNames { get; private set; }
    }
#endif

}
