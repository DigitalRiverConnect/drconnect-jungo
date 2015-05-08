using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using N2.Definitions;
using N2.Edit.FileSystem;
using N2.Edit.Versioning;
using N2.Engine;
using N2.Persistence;
using N2.Persistence.Serialization;

namespace N2.Azure.Replication
{
    [Service(typeof(IReplicationStorage))]
    public class ReplicationStorageFileSystemN2 : IReplicationStorage
    {
        private readonly ContentActivator _activator;
        private readonly IDefinitionManager _definitions;
        private readonly IPersister _persister;
        private readonly IFileSystem _fs;
        private readonly Logger<IReplicationStorage> _logger;
        private readonly string _path = "/App_Data/XmlSync";
        private const string FilePrefix = "page";
        private readonly object _syncLock = new object();

        public ReplicationStorageFileSystemN2(ContentActivator activator, IFileSystemFactory fsfactory, IDefinitionManager definitions, IPersister persister)
        {
            _activator = activator;
            _definitions = definitions;
            _persister = persister;

            string value = (ConfigurationManager.AppSettings["XmlReplication"] ?? "false").ToLowerInvariant();
            if (value.Equals("slave") || value.Equals("master"))
            {
                // only initialize if replication is active
                var storageConfig = (FileSystemNamespace) Enum.Parse(typeof (FileSystemNamespace),
                               ConfigurationManager.AppSettings["AzureReplicationStorageContainerName"] ?? "ReplicationStorageDebug");

                _fs = fsfactory.Create(storageConfig);

                if (_fs.GetType().Name.Contains("Azure"))
                    _path = "/_Xml_Sync_"; // TODO maybe should add TablePrefix?
            }
        }

        #region Private

        private string GetContentItemFilenameUnique(int ID)
        {
            var sb = new StringBuilder(60);
            sb.Append(FilePrefix);
            sb.Append("_");
            sb.Append(ID.ToString());
            sb.Append(".xml");
            return sb.ToString();
        }

        #endregion

        #region IReplicationStorage

        public IEnumerable<ReplicatedItem> GetItems()
        {
            if (_fs == null)
                throw new Exception("no filesystem");

            var items = _fs.GetFiles(_path).Where(f => f.Name.EndsWith(".xml")).Select(f => new ReplicatedItem(f));

            return items;
        }

        public void ExportItem(ContentItem item)
        {
            // no UrlParser and FS -> cannot export Attachments
            var itemXmlWriter = new ItemXmlWriter(_definitions, null, null);
            var exporter = new Exporter(itemXmlWriter);
            var path = _path + '/' + GetContentItemFilenameUnique(item.ID);

            using (var ms = new MemoryStream())
            {
                using (var tw = new StreamWriter(ms))
                {
                    lock (_syncLock)
                    {
                        // Nhibernate doesn't like parallel ?
                        exporter.Export(item, ExportOptions.ExcludeAttachments | ExportOptions.ExcludePages, tw);
                    }
                    // Save to FS
                    ms.Position = 0;
                    _fs.WriteFile(path, ms, item.Published);
                }
            }
        }

        /* public void WriteFile(string name, string data)
        {
            var path = Path.Combine(_path, name);

            using (var ms = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                _fs.WriteFile(path, ms);
            }
        } */

        public IImportRecord SyncItem(ReplicatedItem item)
        {
            lock (this)
            {
                try
                {
                    var reader = new ItemXmlReader(_definitions, _activator, _persister.Repository);
                    var importer = new Importer(null, reader, null);

                    // TODO download
                    var ins = _fs.OpenFile(item.Path, true);
                    var record = importer.Read(ins, item.Path);
                    if (record.RootItem != null)
                        ContentVersion.ReorderBySortOrderRecursive(record.RootItem);
                    return record;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("IMPORT Error: {0} - {1}", item.ID, ex.Message), ex);
                    throw;
                }
            }
        }

        public void DeleteItem(ReplicatedItem replicatedItem)
        {
            _fs.DeleteFile(replicatedItem.Path);
        }

        public void DeleteItem(int ID)
        {
            var path = _path + '/' + GetContentItemFilenameUnique(ID);
            _fs.DeleteFile(path);
        }

        public string ReplicationPath
        {
            get { return _path; }
        }

        #endregion
    }
}