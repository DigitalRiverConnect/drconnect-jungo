using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using N2.Configuration;
using N2.Definitions;
using N2.Edit.FileSystem;
using N2.Engine;
using N2.Persistence;
using N2.Persistence.Finder;
using N2.Persistence.Search;
using N2.Persistence.Serialization;
using N2.Persistence.Xml;
using N2.Security;
using N2.Web;

namespace N2.Azure.Replication
{
    [Service]
    public class ReplicationManager
    {
        public class ReplicationJournal
        {
            private readonly ConcurrentList<string> _log = new ConcurrentList<string>();

            public void Log(string format, params object[] args)
            {
                var s = string.Format(format, args);
                Logger.Info(s);
                _log.Add(s);
            }

            public void LogItem(string format, params object[] args)
            {
                var s = string.Format(format, args);
                Logger.Info(s);
                _log.Add(s);
                Interlocked.Increment(ref AffectedCount);
            }

            public new string ToString()
            {
                return string.Join("\r\n", _log.List);
            }

            public int AffectedCount;
        }

        public bool IsMaster { get; private set; }
        public bool IsSlave { get; private set; }

        private static Logger<ReplicationManager> _logger;
        private readonly IReplicationStorage _repstore;
        private readonly object _syncLock = new object();
        private readonly ISecurityManager _security;  // TODO check if still needed
        private readonly IPersister _persister;
        private readonly IItemFinder _finder;
        private readonly IIndexer _indexer; // nullable
        private readonly IFlushable _flushable;
        private readonly IFileSystem _fileSystem;
        private readonly ReplicationReadLockManager _replicationReadLockManager;
        private readonly ReplicationWriteLockManager _replicationWriteLockManager;
        private readonly string _replicationLogPath;
        private ReplicationJournal _journal;
        private List<UnresolvedLink> _stillUnresolved;
        private Dictionary<int, ContentItem> _newItems;

        public ReplicationManager(
            IPersister persister,
            IItemFinder finder,
            IReplicationStorage repstore,
            IFileSystemFactory fileSystemFactory,
            DatabaseSection dataBaseSection,
            ISecurityManager security
        )
            : this(persister, finder, repstore, fileSystemFactory, dataBaseSection, security, null, null)
        {
            // needed to be able to execute N2 unit tests w/o additional dependecies
        }


        public ReplicationManager(
            IPersister persister,
            IItemFinder finder,
            IReplicationStorage repstore,
            IFileSystemFactory fileSystemFactory,
            DatabaseSection dataBaseSection,
            ISecurityManager security,
            IIndexer indexer, // optional
            IFlushable flushable // optional
            )
        {
            _repstore = repstore;
            _security = security;
            _persister = persister;
            _finder = finder;
            _indexer = indexer;
            _flushable = flushable;

            // detect sync direction from Database Type and double check via config
            string value = ConfigurationManager.AppSettings["XmlReplication"] ?? "false";
            IsSlave = value.Equals("Slave", StringComparison.InvariantCultureIgnoreCase) & (dataBaseSection.Flavour == DatabaseFlavour.Xml);
            IsMaster = value.Equals("Master", StringComparison.InvariantCultureIgnoreCase) & !IsSlave;

            if (IsMaster || IsSlave)
            {
                // only initialize if replication is active
                var storageConfig = (FileSystemNamespace) Enum.Parse(typeof (FileSystemNamespace),
                               ConfigurationManager.AppSettings["AzureReplicationStorageContainerName"] ??
                               "ReplicationStorageDebug");

                _fileSystem = fileSystemFactory.Create(storageConfig);

                // constructing these dependencies to ensure same filesystem and simplify construction
                _replicationWriteLockManager = new ReplicationWriteLockManager(_fileSystem);
                _replicationReadLockManager = new ReplicationReadLockManager(_fileSystem);
            }
            _replicationLogPath = "/_Xml_Sync_Log";
        }

        public void SetUnitTestMode(bool isMaster)
        {
            IsSlave = !isMaster;
            IsMaster = isMaster;
        }

        // get a list of all local page items to be synchronized
        private ConcurrentContentList GetLocalItems()
        {
            lock (_syncLock)
            {
                var result = new ConcurrentContentList(_finder.AllOfType<ContentItem>()
                       .Where(p => p.IsPage && p.IsPublished() && !(p is ISystemNode))); // exclude non managed items

                // add root page (if exists) as that may not be derived from our page base class
                var rootPage = _finder.AllOfType<ContentItem>().FirstOrDefault(i => i is IRootPage);
                if (rootPage != null && !result.Contains(rootPage)) // avoid duplicates
                    result.Insert(0, rootPage);

                return result;
            }
        }

        // perform a full synchronization
        public int Syncronize(bool force = false)
        {
            if (!(IsMaster || IsSlave))
                return 0;

            _journal = new ReplicationJournal();
            lock (_syncLock)
            {
                if (!_replicationWriteLockManager.IsLocked)
                {
                    _stillUnresolved = new List<UnresolvedLink>();
                    _newItems = new Dictionary<int, ContentItem>();
#if DEBUG2
                    (_persister.Repository as XmlContentItemRepository).SetReadOnly(true);
#endif
                    try
                    {
                        if (IsMaster)
                        {
                            // mark intent to lock the replication for write
                            if (_replicationWriteLockManager.Lock() == false)
                                return 0;

                            // Try again later if it is read locked.
                            if (_replicationReadLockManager.IsLocked)
                            {
                                _logger.Info("Read locks exist. Waiting for the next replication scheduled interval.");
                                return 0;
                            }
                        }

                        if (IsSlave && !_replicationWriteLockManager.IsLocked)
                            _replicationReadLockManager.Lock();

                        var localItems = GetLocalItems();                       
                        var remote = _repstore.GetItems().OrderBy(i => i.PublishedDateUtc);
                        if (IsSlave && !remote.Any())
                        {
                            _logger.ErrorFormat("NO REMOTE ITEMS on Synchronize {0} {1}: local items count: {2} instances {3}",
                                               IsMaster ? "Master" : "Slave",
                                               SerializationUtility.GetLocalhostFqdn(),
                                               localItems.Count(),
                                               0);
                            return -1; // never sync down to zero
                        }

                        _logger.InfoFormat("Synchronize {0} {1}: local items count: {2} instances {3}",
                                           IsMaster ? "Master" : "Slave",
                                           SerializationUtility.GetLocalhostFqdn(),
                                           localItems.Count(),
                                           0);

                        // get a list of remotely deleted items and remove from local working copy
                        var itemsToRemove = (IsSlave) ? localItems.List.Where(l => remote.All(r => r.ID != l.ID)).ToList() : new List<ContentItem>();
                        itemsToRemove.ForEach(localItems.Remove);

                        // perform main sychronization and download in parallel - check need import / delete (and remove all localItems that have been handled)
                        Parallel.ForEach(remote, replicatedItem => SyncOneItem(replicatedItem, localItems));

                        // DO THE PUZZLE: combine imported items into new subgraph
                        // first resolve links withing the new items (typically non-page items) - requires no additional local saving
                        // ResolveLinkedItems(_stillUnresolved, (i =>  ? _newItems[i] : null));
                        foreach (var unresolvedLink in _stillUnresolved.ToArray())
                        {
                            if (!_newItems.ContainsKey(unresolvedLink.ReferencedItemID)) continue;
                            unresolvedLink.Setter(_newItems[unresolvedLink.ReferencedItemID]);
                            _stillUnresolved.Remove(unresolvedLink);
                        }
#if DEBUG2
                        (_persister.Repository as XmlContentItemRepository).SetReadOnly(false);
#endif
                        // "COMMIT" phase
                        foreach (var item in _newItems.Values)
                        {
                            if (item.IsPage) // TODO check whether saving pages is sufficient
                            {
                                _logger.Info("new page " + item);
                                // _persister.Save(item); // TODO check persister vs repository saving
                                _persister.Repository.SaveOrUpdate(item);
                                //if (_indexer != null) _indexer.Update(item); // only for pages -> otherwise slows everything down -> move outside of lock
                            }
                        }

                        // RESTORE ALL REMAINING LINKS
                        foreach (var unresolvedLink in _stillUnresolved.ToArray())
                        {
                            var item = _persister.Repository.Get(unresolvedLink.ReferencedItemID);
                            if (item == null) continue;
                            unresolvedLink.Setter(item);
                            _persister.Repository.SaveOrUpdate(unresolvedLink.Item); // ensure proper link, e.g. ParentID
                            _persister.Repository.SaveOrUpdate(item); // this item was not imported, needs local saving
                            _stillUnresolved.Remove(unresolvedLink);
                        }

                        if (_stillUnresolved.Count > 0) 
                        {
                            // now something is really bad - typically an indicator of missing inner nodes
                            _journal.Log("UNRESOLVED count is " + _stillUnresolved.Count);

                            // mitigation - delete all item that cannot be linked during this run 
                            // in hope for success in an upcoming run 
                            foreach (var unresolvedLink in _stillUnresolved.ToArray())
                            {
                                _journal.Log("REMOVING unlinkable item {0} -> {1}", 
                                    unresolvedLink.Item, unresolvedLink.ReferencedItemID);
                                DeleteLocal(unresolvedLink.Item);
                            }                            
                        }
                        //var root = _finder.AllOfType<IRootPage>().Cast<ContentItem>().SingleOrDefault();
                        //if (root != null)
                        //{
                        //    ContentVersion.ReorderBySortOrderRecursive(root);
                        //    UpdateTrailsRecursive(root); // TrailTracker
                        //}

                        // Delete local files that have been removed from remote
                        if (IsSlave)
                            SlaveRemoveDeletedItems(itemsToRemove);

                        // PHASE 3 - check need export (assumes localItems was updated above to have only items not in remote storage)
                        if (IsMaster)
                            Parallel.ForEach(localItems.List, ExportOneItem);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Replication failed", ex);
                        _journal.Log(ex.Message);
#if DEBUG
                        throw;
#endif
                    }
                    finally
                    {
#if DEBUG2
                        (_persister.Repository as XmlContentItemRepository).SetReadOnly(false);
#endif
                        if(IsSlave)
                            _replicationReadLockManager.Unlock();
                    }

                    if (IsMaster)
                        _replicationWriteLockManager.Unlock();

                    _security.ScopeEnabled = true;
                    WriteLog(_journal);

                    if (_journal.AffectedCount > 0)
                    {
                        _logger.InfoFormat("Synchronize Ended {0} {1}: affected count: {2} instances {3}",
                                           IsMaster ? "Master" : "Slave",
                                           SerializationUtility.GetLocalhostFqdn(),
                                           _journal.AffectedCount,
                                           0);

                        if (IsSlave && _flushable != null)
                        {
                            _flushable.Flush(); // Master doesn't change so just flush Slave
                        }
                    }
                }
                else
                {
                    _logger.WarnFormat("Unable to establish a lock for synchronization. Skipping.");
                    return -1;
                }
            }
            return _journal.AffectedCount;
        }

        private void ExportOneItem(ContentItem local)
        {
            _journal.LogItem("export {0} published {1} NEW", local.ID,
                            local.Published.Value.ToUniversalTime());
            _repstore.ExportItem(local);
            // TODO consider naming root/start page root.xml/start.xml vs. host config section
        }

        private void SlaveRemoveDeletedItems(IEnumerable<ContentItem> itemsToRemove)
        {
            if (IsSlave)
            {
                // get a list of all local items w/o corresponding remote and delete them
                foreach (var local in itemsToRemove)
                {
                    // DELETE local copy
                    _journal.LogItem("DELETED REMOTELY {0} published {1}", local.ID,
                                 local.Published.Value.ToUniversalTime());
                    DeleteLocal(local);
                }
            }
        }

        private void DeleteLocal(ContentItem local)
        {
            local.State = ContentState.Deleted;
            _security.ScopeEnabled = false;
            if (_indexer != null) _indexer.Delete(local.ID);
            _persister.Repository.Delete(local); // bypass persister
            _security.ScopeEnabled = true;
        }

        // write summary log file
        private void WriteLog(ReplicationJournal journal)
        {
            var log = journal.ToString();
            using(var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                sw.Write(log);
                sw.Flush();
                ms.Position = 0;

                var name = String.Format("{0}.{1}.{2}.log",
                                         SerializationUtility.GetLocalhostFqdn(),
                                         (IsMaster) ? "M" : "S",
                                         DateTime.UtcNow.ToString("yyMMdd_HHmmss"));
                try
                {
                    if (journal.AffectedCount > 0)
                        _fileSystem.WriteFile(Path.Combine(_replicationLogPath, name), ms);
                }
                catch (Exception e)
                {
                    _logger.Error("Could not write remote blog file " + e.Message);
                    _logger.Info(log);
                }
            }
        }

        // sync local with remote item
        private void SyncOneItem(ReplicatedItem replicatedItem, ConcurrentContentList items)
        {
            var local = items.Get(replicatedItem.ID); // get local match for item
            if (local != null && local.Published.HasValue)
            {
                if (IsSlave)
                // import if newer than local
                {
                    if (replicatedItem.IsNewerThan(local.Published.Value) || !ExistsLocal(local))
                    {
                        _journal.LogItem("import {0} published {1} vs. {2} update",
                            replicatedItem.ID, replicatedItem.PublishedDateUtc,
                            local.Published.Value.ToUniversalTime());
                        ImportItem(replicatedItem);
                    }
                }
                if (IsMaster)
                {
                    if (replicatedItem.IsOlderThan(local.Published.Value))
                    {
                        _journal.LogItem("export {0} published {1} vs. {2} update",
                            replicatedItem.ID, replicatedItem.PublishedDateUtc,
                                     local.Published.Value.ToUniversalTime());
                        _repstore.ExportItem(local);
                    }
                }

                // TODO check version is bigger than replaced version
                items.Remove(local);
            }
            else
            {
                if (IsSlave) // TODO and newer published date
                {
                    _journal.LogItem("import {0} published {1} create",
                        replicatedItem.ID, replicatedItem.PublishedDateUtc);
                    ImportItem(replicatedItem);
                }
                if (IsMaster)
                {
                    // item is not published anymore -> delete replicated item 
                    // TODO signalling - check edge cases, e.g. non-published items that should stay 
                    _journal.LogItem("DELETE item {0} published {1}",
                        replicatedItem.ID, replicatedItem.PublishedDateUtc);
                    _repstore.DeleteItem(replicatedItem);
                }
            }
            replicatedItem.Processed = true;
        }

        // Import one file and update new items and unresolved links
        private void ImportItem(ReplicatedItem item)
        {
            try
            {
                var record = _repstore.SyncItem(item);
                _stillUnresolved.AddRange(record.UnresolvedLinks);

                foreach (var ri in record.ReadItems)
                {
                    _newItems[ri.ID] = ri;
                }
            }
            catch (Exception e)
            {
                _logger.Error("IMPORT Error: " + e.Message, e);
                _journal.Log(e.Message);
            }
        }

        private bool ExistsLocal(ContentItem item)
        {
            var xr = _persister.Repository as XmlContentItemRepository; // TODO remove dependency
            if (xr == null)
                return true;

            return xr.ExistsLocal(item);
        }

        #region Trail Tracker
#if TRAILS
        private void UpdateTrailsRecursive(ContentItem parent)
        {
            if (parent != null)
            {
                UpdateChildrenRecursive(parent);
            }
        }

        private static int UpdateAncestralTrailOf(ContentItem item)
        {
            string trail = item.Parent.GetTrail();
            if (item.AncestralTrail != trail)
            {
                item.AncestralTrail = trail;
                return 1;
            }
            return 0;
        }

        private static int UpdateChildrenRecursive(ContentItem parent)
        {
            int numberOfUpdatedItems = 0;
            foreach (var child in parent.Children)
            {
                numberOfUpdatedItems += UpdateAncestralTrailOf(child);
                numberOfUpdatedItems += UpdateChildrenRecursive(child);
            }
            return numberOfUpdatedItems;
        }
#endif
        #endregion

    }
}
