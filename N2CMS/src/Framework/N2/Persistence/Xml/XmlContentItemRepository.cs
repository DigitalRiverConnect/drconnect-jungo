using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using N2.Configuration;
using N2.Definitions;
using N2.Edit.FileSystem;
using N2.Edit.Versioning;
using N2.Engine;
using N2.Persistence.NH;
using N2.Persistence.Serialization;

namespace N2.Persistence.Xml
{
    /// <summary>Provides a service to store content items as loose XML files, rather than using a database.</summary>

    [Service(typeof(IContentItemRepository), Configuration = "xml")]
    [Service(typeof(IRepository<ContentItem>), Configuration = "xml", Replaces = typeof(ContentItemRepository))]
    public class XmlContentItemRepository : XmlRepository<ContentItem>, IContentItemRepository
    {
        private readonly Exporter exporter;
        private readonly ContentActivator activator;
        private readonly IDefinitionManager definitions;
        private static Logger<XmlContentItemRepository> _logger;
        private bool _readonly = false;

        public XmlContentItemRepository(
            ContentActivator activator, IFileSystem fs, IDefinitionManager definitions,
            DatabaseSection config, ConnectionStringsSection connectionStrings)
            : base(config, connectionStrings)
        {
            _logger.Debug("NEW XmlContentItemRepository");
            this.activator = activator;
            this.definitions = definitions;
            exporter = new Exporter(new ItemXmlWriter(this.definitions, null, fs));

            LoadAll();
        }

        public void SetReadOnly(bool value)
        {
            _readonly = value;
        }

        public override ContentItem Get(object id)
        {
            // TODO check for modified based on TimeToLive
            var item = base.Get(id);
            if (item == null && id is Int32)
            {
                var intID = (int) id;
                foreach (var page in AllContentPages)
                {
                    ContentItem match = N2.Find.EnumerateChildren(page).FirstOrDefault(c => c.ID == intID);
                    if (match != null && match.ID == intID)
                    {
                        _logger.Debug("resolved id via recursive search to " + match);
                        Database[intID] = match;
                        return match;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// load all content item files into the memory cache
        /// </summary> 
        private void LoadAll()
        {
            lock (this)
            {
                _logger.Info("Loading Xml Files");
                var reader = new ItemXmlReader(definitions, activator, this);
                var persister = new ContentPersister(null /* what is the contentsource? */, this);
                var importer = new Importer(persister, reader, null);

                var records = new List<IImportRecord>();
                var files = Directory.GetFileSystemEntries(DataDirectoryPhysical, "c-*.xml");

                records.AddRange(from f in files select importer.Read(f));

                // resolve links
                var itemsByid = (from x in records.SelectMany(f => f.ReadItems)
                                 group x by x.ID
                                     into y
                                     select new { ID = y.Key, ContentItem = y.First() })
                    .ToLookup(f => f.ID);

                //var stillUnresolved = new List<UnresolvedLink>();
                foreach (var unresolvedLink in records.SelectMany(f => f.UnresolvedLinks))
                    if (itemsByid.Contains(unresolvedLink.ReferencedItemID))
                        unresolvedLink.Setter(itemsByid[unresolvedLink.ReferencedItemID].First().ContentItem);
                    else
                        _logger.ErrorFormat("Unresolved on Load {0} -> {1}", unresolvedLink.Item.ID, unresolvedLink.ReferencedItemID);
                        //stillUnresolved.Add(unresolvedLink);

                foreach (var x in itemsByid.Select(f => f.First().ContentItem))
                {
                    ContentVersion.ReorderBySortOrderRecursive(x);
                    if (x.IsPage)
                        Database.Add(x.ID, x);
                }

                var root = AllContentPages.FirstOrDefault(i => i is IRootPage);
                if (root != null)
                {
                    ContentVersion.ReorderBySortOrderRecursive(root);
                }

                foreach (var item in Database.Values.Where(i => (i != root) && i.Parent == null).ToArray())
                {
                    _logger.Error("Missing Parent for " + item + " removing from memory and local copy");
                    Delete(item);
                }
            }
        }

        public IEnumerable<ContentItem> AllContentPages
        {
            get
            {
                foreach (var key in Database.Keys)
                {
                    if (Database[key] == null)
                        _logger.ErrorFormat("Missing Dictionary entry for id " + key);
                }

                return Database.Values.Where(c => c != null && c.IsPage);
            } 
        }

        public IEnumerable<ContentItem> AllContentItems
        {
            get { return AllContentPages.SelectMany(AllChildren); }
        }

        public override long Count()
        {
            return AllContentItems.Count();
        }

        public IEnumerable<ContentItem> AllChildren(ContentItem item)
        {
            var ret = new List<ContentItem>();

            // DON'T will skip root node - if (!(item is ISystemNode))
            ret.Add(item);
            foreach (var child in item.Children.Where(c => !c.IsPage && !(c is ISystemNode)))
            {
                ret.AddRange(AllChildren(child));
            }
            return ret;
        }

        public IEnumerable<T> AllOfType<T>() where T : ContentItem
        {
            if (typeof(T) is IPage)
                return Database.OfType<T>();

            return AllContentItems.OfType<T>();
        }

        #region IContentItemRepository members

        public IEnumerable<DiscriminatorCount> FindDescendantDiscriminators(ContentItem ancestor)
        {
            var discriminators = new Dictionary<string, int>();
            if (ancestor != null)
            {
                var exploredList = new List<ContentItem>();
                var exploreList = new Queue<ContentItem>();

                exploreList.Enqueue(ancestor);
                while (exploreList.Count > 0)
                {
                    var current = exploreList.Dequeue();
                    if (exploredList.Contains(current))
                        continue;
                    exploredList.Add(current);

                    var discriminator = definitions.GetDefinition(current).Discriminator;
                    if (discriminators.ContainsKey(discriminator))
                        discriminators[discriminator]++;
                    else
                        discriminators.Add(discriminator, 1);

                    foreach (var child in current.Children)
                        exploreList.Enqueue(child);
                }
            }
            else
            {
                foreach (var discriminator in AllContentItems.Select(current => definitions.GetDefinition(current).Discriminator))
                {
                    if (discriminators.ContainsKey(discriminator))
                        discriminators[discriminator]++;
                    else
                        discriminators.Add(discriminator, 1);
                }
            }
            return discriminators.Select(x => new DiscriminatorCount { Count = x.Value, Discriminator = x.Key }).OrderByDescending(dc => dc.Count);
        }

        public IEnumerable<ContentItem> FindDescendants(ContentItem ancestor, string discriminator)
        {
            if (ancestor == null)
                return (from x in AllContentItems
                        where definitions.GetDefinition(x).Discriminator == discriminator
                        select x).ToList(); // force immediate execution of lambda

            return (from x in AllContentItems
                    where (x.ID == ancestor.ID || x.AncestralTrail.StartsWith(ancestor.AncestralTrail))
                          && definitions.GetDefinition(x).Discriminator == discriminator
                    select x).ToList(); // force immediate execution of lambda
        }

        public IEnumerable<ContentItem> FindReferencing(ContentItem linkTarget)
        {
            return (from x in AllContentItems
                    where x.Details.Any(d => d.LinkedItem.ID == linkTarget.ID)
                       || x.DetailCollections.Any(dc => dc.Details.Any(dd => dd.LinkedItem.ID == linkTarget.ID))
                    select x).ToList(); // force immediate execution of lambda
        }

        public int RemoveReferencesToRecursive(ContentItem target)
        {
            return 0; // TODO fix NPE below

            //var count = 0;
            //try
            //{
            //    var toUpdate = new HashSet<ContentItem>();
            //    var items = AllContentItems.Where(db => (db.AncestralTrail != null) && db.AncestralTrail.StartsWith(target.AncestralTrail));
            //    var list = AllContentItems.SelectMany(x => x.Details).ToList();

            //    list = list.Where(
            //            d =>
            //            (d.LinkedItem != null) &&
            //            (d.LinkedItem.ID == target.ID || 
            //            items.Any(ci => ci.ID == d.LinkedItem.ID)))
            //            .ToList();


            //    foreach (var detail in list)
            //    {
            //        Logger.DebugFormat("Removing detail {0}:{1} from contentItem {2}:{3}.", detail.ID, detail.Name, detail.LinkedItem.ID, detail.LinkedItem.Name);
            //        toUpdate.Add(detail.EnclosingItem);
            //        detail.AddTo((ContentItem)null);
            //        ++count;
            //    }
            //    foreach (var item in toUpdate)
            //        SaveOrUpdate(item);

            //}
            //catch (Exception ex)
            //{
            //    Logger.Error("RemoveReferencesToRecursive " + ex.Message);                
            //}
            //return count;
        }

        #endregion

        public override IEnumerable<ContentItem> Find(IParameter parameters)
        {
            var result = from w in Database
                         let x = w.Value
                         where parameters.IsMatch(x)
                         select x;

            var pc = parameters as ParameterCollection;
            if (pc != null)
            {
                if (pc.Order != null && pc.Order.HasValue)
                {
                    result = pc.Order.Descending ? result.OrderByDescending(e => e[pc.Order.Property])
                                                 : result.OrderBy(e => e[pc.Order.Property]);
                }

                if (pc.Range != null)
                {
                    if (pc.Range.Skip > 0)
                        result = result.Skip(pc.Range.Skip);
                    if (pc.Range.Take > 0)
                        result = result.Take(pc.Range.Take);
                }
            }

            return result;
        }

        public override void SaveOrUpdate(ContentItem item)
        {
            if (_readonly)
                throw new Exception("Repository is in read only mode");

            try
            {
                lock (this)
                {
                    if (item.ID == 0)
                    {
                        if (Database.All(x => x.Value != item))
                            item.ID = Database.Count > 0 ? AllContentItems.Max(f => f.ID) + 1 : 1;
                    }

                    var old = Database.ContainsKey(item.ID) ? Database[item.ID] : null;
                    if (old != null && !System.Object.ReferenceEquals(old, item))
                    {


                        // ALTERNATIVE (old as IUpdatable<ContentItem>).UpdateFrom(item);
                        var oldParent = old.Parent;
                        if (oldParent != null)
                        {
                            _logger.InfoFormat("Removing old version of {0}:{1} from {2}:{3}", item.ID, GetContentItemName(item), oldParent.ID, GetContentItemName(oldParent));
                            old.AddTo(null);
                            item.AddTo(oldParent); // will be overridden by final run of unresolved links
                        }

                        var oldChildPageIds = old.Children.Where(i => i.IsPage).Select(i => i.ID).ToArray();
                        foreach (var oldChildPageId in oldChildPageIds)
                        {
                            if (Database.ContainsKey(oldChildPageId))
                            {
                                var oldChildPage = Database[oldChildPageId];
                                if (item.Children.Any(i => i.ID == oldChildPageId))
                                {
                                    var itemToRemove = item.Children.First(i => i.ID == oldChildPageId);
                                    item.Children.Remove(itemToRemove);
                                }

                                _logger.InfoFormat("Transfering children from old to new version of {0}:{1}", item.ID, GetContentItemName(item));
                                oldChildPage.AddTo(item);
                                old.Children.Remove(oldChildPage);
                            }
                            else
                            {
                                _logger.Error(string.Format("Unable to link child page {0} to item {1}.", oldChildPageId, item.ID));
                            }
                        }
                        ContentVersion.ReorderBySortOrderRecursive(item);
#if DEBUG
                        var oldChildren = old.Children.Select(c => c.ID).ToList();
                        var newChildren = item.Children.Select(c => c.ID).ToList();
                        if (!(newChildren.All(oldChildren.Contains) && oldChildren.All(newChildren.Contains)))
                            _logger.DebugFormat("Replacing data {0}  - children {1} -> {2}",
                                item.ID, string.Join(",", oldChildren), string.Join(",", newChildren));
#endif
                        Delete(old);
                    }

                    _logger.DebugFormat("Adding or Replacing item {0}:{1}", item.ID, item.Name);
                    Database[item.ID] = item;

                    if (item.IsPage)
                    {
                        InternalDeleteFiles(GetPath(item, true));
                        var path = Path.Combine(DataDirectoryPhysical, GetPath(item));
                        _logger.InfoFormat("Creating file \"{0}\"", path);
                        XmlPersistenceUtility.SaveXml(exporter, item, path);
                    }
                    else if (item.Parent != null)
                    {
                        SaveOrUpdate(item.Parent); // TODO optimize multiple saves during import by creating a real xml transaction on the page level
                    }
                    else
                    {
                        throw new ArgumentException("Invalid item: not a page and parent = null");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error("SaveOrUpdate failed on " + item, ex);
            }

        }

        private static string GetContentItemName(ContentItem item)
        {
            return string.IsNullOrEmpty(item.Name) ? item.GetType().Name : item.Name;
        }

        public override void Delete(ContentItem entity)
        {
            if (_readonly)
                throw new Exception("Repository is in read only mode");
            var item = entity;
            if (item != null)  // avoid search below
            {
                item.State = ContentState.Deleted; // change state - just in case item is still referenced 

                RemoveReferencesToRecursive(item);

                // ensure item gets removed from memory - has no further references ... e.g. parents
                if (item.Parent != null && item.Parent.Children.Contains(item))
                {
                    item.Parent.Children.Remove(item); // compare ContentItem.AddTo(null)
                }

                // handle cascading deletion
                // make a working copy of the list, then delete
                List<ContentItem> children = item.Children.ToList();
                item.Children.Clear();
                foreach (var child in children)
                {
#if DEBUG
                    _logger.Debug("Delete " + child);
#endif
                    Delete(child);

                }
            }

            base.Delete(entity);
        }

        public bool ExistsLocal(ContentItem item)
        {
            var path = Path.Combine(DataDirectoryPhysical, GetPath(item));
            return File.Exists(path);
        }

        public override string GetPath(ContentItem item, bool wild = false)
        {
            var sb = new StringBuilder(60);
            sb.Append("c-");
            sb.Append(item.ID.ToString().PadLeft(6, '0'));
            sb.Append('-');
            if (wild)
                sb.Append('*');
            else
            {
                var typeName = item.GetType().Name;
                if (typeName.EndsWith("Proxy")) typeName = typeName.Substring(0, typeName.Length - 5); // remove Proxy from Name
                XmlPersistenceUtility.AppendTextSafe(sb, typeName);
                if (!string.IsNullOrEmpty(item.Name) && !item.Name.Equals(item.ID.ToString()))
                {
                    sb.Append('-');
                    XmlPersistenceUtility.AppendTextSafe(sb, item.Name);
                }
            }
            sb.Append(".xml");
            return sb.ToString();
        }
    }

    public static class XmlPersistenceUtility
    {
        public static void SaveXml(Exporter exporter, ContentItem item, string fileName)
        {
            string tempFileName = Path.GetTempFileName();
            // write out the data to temporary file
            using (var tw = File.CreateText(tempFileName))
                exporter.Export(item, ExportOptions.ExcludeAttachments | ExportOptions.ExcludePages, tw);

            MoveOrReplaceFile(tempFileName, fileName);

            // make file attrs match item
            File.SetCreationTime(fileName, item.Created);
            File.SetLastWriteTime(fileName, item.Published.HasValue ? item.Published.Value : item.Updated);
            File.SetAttributes(fileName, FileAttributes.Normal);
        }

        public static void AppendTextSafe(StringBuilder sb, string name)
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (var c in name)
                if (!invalidFileNameChars.Contains(c))
                    sb.Append(c);
        }


        // http://stackoverflow.com/questions/8958094/reliable-file-saving-file-replace-in-a-busy-environment
        public static void MoveOrReplaceFile(string source, string destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (File.Exists(destination))
            {
                // File.Replace does not work across volumes
                if (Path.GetPathRoot(Path.GetFullPath(source)) == Path.GetPathRoot(Path.GetFullPath(destination)))
                {
                    string backup = destination + ".bak";
                    File.Delete(backup);

                    File.Replace(source, destination, backup, true);
                    try
                    {
                        File.Delete(backup);
                    }
                    catch
                    {
                        // optional: filesToDeleteLater.Add(backup);
                    }
                }
                else
                {
                    File.Copy(source, destination, true);
                }
            }
            else
            {
                File.Move(source, destination);
            }
        }
    }
}
