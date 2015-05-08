using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using N2;
using N2.Definitions;
using N2.Details;
using N2.Edit.FileSystem;
using N2.Edit.Installation;
using N2.Engine;
using N2.Persistence;
using N2.Persistence.Finder;
using N2.Plugin.Scheduling;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    /// <summary>
    /// Experimental Agent that fixes broken links
    /// by Stefan Weber - work in progress 
    /// TODO: handle (draft) versions
    /// TODO: handle link to contents
    /// </summary>
    [ScheduleExecution(interval: 30, unit: TimeUnit.Minutes, Configuration = "sql")]
    public class LinkFixupScheduledAction: ScheduledAction
    {
        //private readonly Logger<LinkFixupScheduledAction> logger;

        readonly IItemFinder _finder;
        readonly IPersister _persister;
        readonly IHost _host;
        readonly HashSet<string> _fixedItems = new HashSet<string>();
        private String _base;
        private readonly IFileSystem _fs;
        private readonly RequestPathProvider _rpp;

        public LinkFixupScheduledAction(IItemFinder finder, IPersister persister, IHost host, IFileSystem fs, IEngine engine)
        {
            _finder = finder;
            _persister = persister;
            _host = host;
            _fs = fs;
            _rpp = engine.Resolve<RequestPathProvider>();
        }

        /// <summary>
        /// Locate a file by name on the current IFileSystem using recursion
        /// TODO - use a find cache 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        private string FindFile(string path, string fname)
        {
            var fpath = path.Replace('\\', '/').TrimEnd('/') + '/' + fname; // TODO extract method

            if (_fs.FileExists(fpath))
                return fpath;

            foreach (var dir in _fs.GetDirectories(path))
            {
                var result = FindFile(dir.VirtualPath, fname); // recurse search
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Apply fixes to a given content item.
        /// </summary>
        /// <param name="item">Item to be fixed up.</param>
        /// <returns>true if changed made and persistence update needed</returns>
        private bool Fix(ContentItem item)
        {
            bool changed = false;
            const string root = "/upload/";
            const string http = "http";

            // iterate over properties   
            // TODO create a cross reference of images to be shown in image folders
            foreach (var pi in item.GetContentType().GetProperties())
            {
                if (pi.CanRead == false || pi.CanWrite == false || pi.PropertyType != typeof(string))
                    continue;

                // rebase images 
                if (pi.GetCustomAttributes(typeof(EditableImageExAttribute), false).Any() || 
                    pi.GetCustomAttributes(typeof(EditableImageAttribute), false).Any())
                {
                    string url = pi.GetValue(item, null) as string ?? "";
                    string urlNew = url;

                    if (String.IsNullOrEmpty(url) || url.StartsWith(http))
                        continue; // skip - TODO check for own domain and remove protocol

                    if (url.StartsWith(_base))
                        urlNew = "~/" + url.Substring(_base.Length);

                    if (url.StartsWith(root)) // TODO use config
                        urlNew = "~" + url;

                    // TODO ensure only forward slashes

                    // check file exists, othewise try to find it
                    if (!_fs.FileExists(urlNew))
                    {
                        //logger.Warn(item.ID + " " + pi.Name + " Not found " + urlNew);

                        urlNew = FindFile("~" + root, Path.GetFileName(urlNew));
                        if (urlNew == null)
                            continue;
                    }

                    if (!url.Equals(urlNew))
                    {
                        //logger.Debug(item.ID + " " + pi.Name + " " + url + " -> " + urlNew);
                        pi.SetValue(item, urlNew, null);
                        changed = true;
                        _fixedItems.Add(pi.Name);
                    }
                }
                
                // rebase links
                if (pi.GetCustomAttributes(typeof(EditableUrlAttribute), false).Any())
                {
                    string url = pi.GetValue(item, null) as string ?? "";
                   
                    if (String.IsNullOrEmpty(url) || url.StartsWith(http))
                        continue; // skip - TODO check for own domain and remove protocol

                    //logger.Debug(item.ID + " " + pi.Name + " " + url);

                    var qindex = url.IndexOf("?", StringComparison.Ordinal);
                    if (qindex > 0)
                    {
                        // parameterized url
                        // TODO restrict to known cases
                        string urlNew = url.Substring(qindex);
                        //logger.Debug(item.ID + " " + pi.Name + " " + url + " -> " + urlNew);
                        pi.SetValue(item, urlNew, null);
                        changed = true;
                        _fixedItems.Add(pi.Name);
                    }
                    
                    if (qindex < 0)
                    {
                        // try to resolve local url
			            var	path = _rpp.ResolveUrl(url);
                        if (path.IsEmpty())
                        {
                            //logger.Warn("** Not found: " + url);
                        }
                    }
                }

            }

            return changed;
        }

        private void FixType(Type t)
        {
            // apply fixes
            using (var tx = _persister.Repository.BeginTransaction())
            {

                foreach (var item in _finder.Where.Type.Eq(t)
                    .OrderBy.ID.Asc.Select().Where(Fix))
                {
                    _persister.Repository.SaveOrUpdate(item); // TODO handle versions - this only touches the non-versioned item
                }


                _persister.Repository.Flush();
                tx.Commit();
            }
        } 

        public override void Execute()
        {
            // get app root
            var root = _persister.Get(_host.DefaultSite.RootItemID);
            _base = root[InstallationManager.InstallationAppPath] as string ?? "";
            //logger.Info("LinkFixupScheduledAction BASE:" + _base + " actual " + Url.ToAbsolute("~/"));

            // identify types that need inspection
            /*var types = (from def in _definitions.GetDefinitions()
                         from attr in def.Attributes
                         where attr is EditableImageAttribute || attr is EditableImageExAttribute || attr is EditableLinkAttribute || attr is EditableUrlAttribute
                         select def.ItemType).ToList();
             */

            /* TODO foreach (var def in _definitions.GetDefinitions())
            {
                foreach (var attr in def.Attributes)
                {
                    logger.Debug(attr.ToString());
                }
            }*/

            //var types = new Type[]()  PromotionOffer, NavLink];
            //types.ForEach(FixType);
            //FixType(typeof(PromotionOffer));
            //FixType(typeof(NavLink));

            //logger.Info("LinkFixupScheduledAction DONE:");
            //foreach (var s in _fixedItems) { logger.Debug(s); }
        }
 
    }
}

