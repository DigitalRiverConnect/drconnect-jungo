using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using N2.Persistence.Serialization;


namespace N2.Azure {
	public class AzureFileSystem : IStorageProvider
	{
        public const string FolderEntry = "$$$FOLDER$$$.$$$"; //"$$$ORCHARD$$$.$$$"; // hidden dummy file created for each folder

        public string ContainerName { get; protected set; }

        private readonly CloudStorageAccount _storageAccount;
        private readonly string _root;
        private readonly string _absoluteRoot;
        public CloudBlobClient BlobClient { get; private set; }
        public CloudBlobContainer Container { get; private set; }

		// RoleEnvironment.GetConfigurationSettingValue("DataConnectionString")

		public AzureFileSystem(string connection, string containerName, string root, bool isPrivate)
            : this(containerName, root, isPrivate, CloudStorageAccount.Parse(connection)) {
        }

        public AzureFileSystem(string containerName, string root, bool isPrivate, CloudStorageAccount storageAccount) {
            // Setup the connection to custom storage accountm, e.g. Development Storage
            _storageAccount = storageAccount;
            ContainerName = containerName;
            _root = String.IsNullOrEmpty(root) ? "": root + "/";
            _absoluteRoot = Combine(Combine(_storageAccount.BlobEndpoint.AbsoluteUri, containerName), _root);

            //using ( new HttpContextWeaver() ) 
            {

                BlobClient = _storageAccount.CreateCloudBlobClient();
                // Get and create the container if it does not exist
                // The container is named with DNS naming restrictions (i.e. all lower case)
                Container = BlobClient.GetContainerReference(ContainerName);

                Container.CreateIfNotExists();

                Container.SetPermissions(isPrivate
                                             ? new BlobContainerPermissions
                                                   {PublicAccess = BlobContainerPublicAccessType.Off}
                                             : new BlobContainerPermissions
                                                   {PublicAccess = BlobContainerPublicAccessType.Blob}); // deny listing 
            }

        }

        private static void EnsurePathIsRelative(string path) {
            if ( path.StartsWith("/") || path.StartsWith("http://") || path.StartsWith("https://") )
                throw new ArgumentException("Path must be relative");
        }

        public string Combine(string path1, string path2) {
            if ( path1 == null) {
                throw new ArgumentNullException("path1");
            }

            if ( path2 == null ) {
                throw new ArgumentNullException("path2");
            }

            if ( String.IsNullOrEmpty(path2) ) {
                return path1;
            }

            if ( String.IsNullOrEmpty(path1) ) {
                return path2;
            }

            if ( path2.StartsWith("http://") || path2.StartsWith("https://") ) {
                return path2;
            }

            var ch = path1[path1.Length - 1];

            if (ch != '/') {
                return (path1.TrimEnd('/') + '/' + path2.TrimStart('/'));
            }

            return (path1 + path2);
        }

        public IStorageFile GetFile(string path) {
            EnsurePathIsRelative(path);

            Container.EnsureBlobExists(String.Concat(_root, path));
            return new AzureBlobFileStorage(Container.GetBlockBlobReference(String.Concat(_root, path)), _absoluteRoot, true);
        }

        public bool FileExists(string path) {
            return Container.BlobExists(String.Concat(_root, path));
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {
            path = path ?? String.Empty;
            
            EnsurePathIsRelative(path);

            string prefix = Combine(Combine(Container.Name, _root), path);
            
            if ( !prefix.EndsWith("/") )
                prefix += "/";

            return BlobClient
                .ListBlobs(prefix, false, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Where(blobItem => !blobItem.Uri.AbsoluteUri.EndsWith(FolderEntry))
                .Select(blobItem => new AzureBlobFileStorage(blobItem, _absoluteRoot))
                .ToArray();
        }

	    public bool DirectoryExists(string path)
	    {
            return Container.DirectoryExists(String.Concat(_root, path));
	    }

	    public IEnumerable<IStorageFolder> ListFolders(string path) {
            path = path ?? String.Empty;

            EnsurePathIsRelative(path);
            var rootPath = String.Concat(_root, path);

            // return root folders
            if (rootPath == String.Empty)
            {
                return Container.ListBlobs()
                    .OfType<CloudBlobDirectory>()
                    .Select<CloudBlobDirectory, IStorageFolder>(d => new AzureBlobFolderStorage(d, _absoluteRoot))
                    .ToList();
            }

            if (!Container.DirectoryExists(rootPath))
            {
                try {
                    CreateFolder(path);
                }
                catch ( Exception ex ) {
                    throw new ArgumentException(string.Format("The folder could not be created at path: {0}. {1}",
                                                                path, ex));
                }
            }

            return Container.GetDirectoryReference(rootPath)
                .ListBlobs()
                .OfType<CloudBlobDirectory>()
                .Select<CloudBlobDirectory, IStorageFolder>(d => new AzureBlobFolderStorage(d, _absoluteRoot))
                .ToList();
        }

        public bool TryCreateFolder(string path) {
            try {
                if (!Container.DirectoryExists(String.Concat(_root, path))) {
                    CreateFolder(path);
                    return true;
                }

                // return false to be consistent with FileSystemProvider's implementation
                return false;
            }
            catch {
                return false;
            }
        }

        public void CreateFolder(string path) {
            EnsurePathIsRelative(path);
            Container.EnsureDirectoryDoesNotExist(String.Concat(_root, path));

            // Creating a virtually hidden file to make the directory an existing concept
            CreateFile(Combine(path, FolderEntry));

            int lastIndex;
            while ((lastIndex = path.LastIndexOf('/')) > 0) {
                path = path.Substring(0, lastIndex);
                if(!Container.DirectoryExists(String.Concat(_root, path))) {
                    CreateFile(Combine(path, FolderEntry));
                }
            }
        }

        public void DeleteFolder(string path) {
            EnsurePathIsRelative(path);

            Container.EnsureDirectoryExists(String.Concat(_root, path));
            foreach ( var blob in Container.GetDirectoryReference(String.Concat(_root, path)).ListBlobs() ) {
                var blockBlob = blob as CloudBlockBlob;
                if (blockBlob != null)
                    blockBlob.Delete();

                if (blob is CloudBlobDirectory)
                    DeleteFolder(blob.Uri.ToString().Substring(Container.Uri.ToString().Length + 1 + _root.Length));
            }
        }

        public void RenameFolder(string path, string newPath) {
            EnsurePathIsRelative(path);
            EnsurePathIsRelative(newPath);

            if ( !path.EndsWith("/") )
                path += "/";

            if ( !newPath.EndsWith("/") )
                newPath += "/";

            foreach (var blob in Container.GetDirectoryReference(_root + path).ListBlobs()) {
                if (blob is CloudBlockBlob)
                {
                    string filename = Path.GetFileName(blob.Uri.ToString());
                    string source = String.Concat(path, filename);
                    string destination = String.Concat(newPath, filename);
                    RenameFile(source, destination);
                }

                if (blob is CloudBlobDirectory) {
                    string foldername = blob.Uri.Segments.Last();
                    string source = String.Concat(path, foldername);
                    string destination = String.Concat(newPath, foldername);
                    RenameFolder(source, destination);
                }
            }
        }

        public void DeleteFile(string path) {
            EnsurePathIsRelative(path);
            
            Container.EnsureBlobExists(Combine(_root, path));
            var blob = Container.GetBlockBlobReference(Combine(_root, path));
            blob.Delete();
        }

        public void RenameFile(string path, string newPath) {
            EnsurePathIsRelative(path);
            EnsurePathIsRelative(newPath);

            Container.EnsureBlobExists(String.Concat(_root, path));
            Container.EnsureBlobDoesNotExist(String.Concat(_root, newPath));

            var blob = Container.GetBlockBlobReference(String.Concat(_root, path));
            var newBlob = Container.GetBlockBlobReference(String.Concat(_root, newPath));
            newBlob.StartCopyFromBlob(blob);
            blob.Delete();
        }

        public IStorageFile CreateFile(string path, DateTime? lastWriteTime = null) {
            EnsurePathIsRelative(path);
            
            if ( Container.BlobExists(String.Concat(_root, path)) ) { 
                // MAB delete the file if it already exists
                DeleteFile(path);
                //throw new ArgumentException("File " + path + " already exists");
            }

            // create all folder entries in the hierarchy
            int lastIndex;
            var localPath = path;
            while ((lastIndex = localPath.LastIndexOf('/')) > 0) {
                localPath = localPath.Substring(0, lastIndex);
                var folder = Container.GetBlockBlobReference(String.Concat(_root, Combine(localPath, FolderEntry)));
                folder.OpenWrite().Dispose();
            }

            var blob = Container.GetBlockBlobReference(String.Concat(_root, path));
            var contentType = GetContentType(path);
            if (!String.IsNullOrWhiteSpace(contentType)) {
                blob.Properties.ContentType = contentType;

                if (IsCacheable(contentType))
                    blob.Properties.CacheControl = "max-age=1800, public";
            }

            blob.Metadata.Add("UploadedFromNode", SerializationUtility.GetLocalhostFqdn());
            if (lastWriteTime.HasValue) blob.Metadata.Add("LastWriteTime", SerializationUtility.ToUniversalString(lastWriteTime));

            using (var memoryStream = new MemoryStream(new byte[0]))
            {
                blob.UploadFromStream(memoryStream);
            }

            return new AzureBlobFileStorage(blob, _absoluteRoot);
        }

        private bool IsCacheable(string contentType)
        {
            var ctl = contentType.ToLowerInvariant();
            return ctl.Contains("javascript") || ctl.Contains("css") || ctl.Contains("image");
        }

        public string GetPublicUrl(string path) {
            EnsurePathIsRelative(path);

            return Container.BlobExists(String.Concat(_root, path)) ? Container.GetBlockBlobReference(String.Concat(_root, path)).Uri.ToString() : null;
        }

        /// <summary>
        /// Returns the mime-type of the specified file path, looking into IIS configuration and the Registry
        /// </summary>
        private string GetContentType(string path) {
            string extension = Path.GetExtension(path);
            if (String.IsNullOrWhiteSpace(extension)) {
                return "application/unknown";
            }

            try {
                try {
                    string applicationHost = Environment.ExpandEnvironmentVariables(@"%windir%\system32\inetsrv\config\applicationHost.config");
                    string webConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null).FilePath;

                    // search for custom mime types in web.config and applicationhost.config
                    foreach (var configFile in new[] {webConfig, applicationHost}) {
                        if (File.Exists(configFile)) {
                            var xdoc = XDocument.Load(configFile);
                            var mimeMap = xdoc.XPathSelectElements("//staticContent/mimeMap[@fileExtension='" + extension + "']").FirstOrDefault();
                            if (mimeMap != null) {
                                var mimeType = mimeMap.Attribute("mimeType");
                                if (mimeType != null) {
                                    return mimeType.Value;
                                }
                            }
                        }
                    }
                }
                catch {
                    // ignore issues with web.config to fall back to registry
                }

                // search into the registry
                var regKey = Registry.ClassesRoot.OpenSubKey(extension.ToLower());
                if (regKey != null) {
                    var contentType = regKey.GetValue("Content Type");
                    if (contentType != null) {
                        return contentType.ToString();
                    }
                }
            }
            catch {
                // if an exception occured return application/unknown
                return "application/unknown";
            }

            return "application/unknown";
        }

        private class AzureBlobFileStorage : IStorageFile {
            private CloudBlockBlob _blob;
            private readonly string _rootPath;

            public AzureBlobFileStorage(CloudBlockBlob blob, string rootPath, bool fetchAttributes = false) {
                _blob = blob;
                if(fetchAttributes)
                    _blob.FetchAttributes();
                _rootPath = rootPath;
            }

            public string GetPath() {
                return _blob.Uri.ToString().Substring(_rootPath.Length).Trim('/');
            }

            public string GetName() {
                return Path.GetFileName(GetPath());
            }

            public long GetSize() {
                if (_blob.Properties.Length <= 0) 
                    _blob.FetchAttributes(); // refresh for new files
                return _blob.Properties.Length;
            }

            public DateTime GetLastUpdated()
            {
                string value;
                if (_blob.Metadata.TryGetValue("LastWriteTime", out value))
                {
                    return SerializationUtility.UniversalStringToDateTime(value);
                }

                if (_blob.Properties.LastModified != null)
                    return _blob.Properties.LastModified.Value.UtcDateTime;

                return DateTime.MinValue; // TODO check
            }

            public string GetFileType() {
                return Path.GetExtension(GetPath());
            }

            public Stream OpenRead() {
                return _blob.OpenRead();
            }

            public Stream OpenWrite() {
                return _blob.OpenWrite();
            }

            public Stream CreateFile() {
                // as opposed to the File System implementation, if nothing is done on the stream
                // the file will be emptied, because Azure doesn't implement FileMode.Truncate
                _blob.DeleteIfExists();
                _blob = _blob.Container.GetBlockBlobReference(_blob.Uri.ToString());
                _blob.OpenWrite().Dispose(); // force file creation

                return OpenWrite();
            }
        }

        private class AzureBlobFolderStorage : IStorageFolder {
            private readonly CloudBlobDirectory _blob;
            private readonly string _rootPath;

            public AzureBlobFolderStorage(CloudBlobDirectory blob, string rootPath) {
                _blob = blob;
                // _blob.Container.FetchAttributes();
                _rootPath = rootPath;
            }

            public string GetName() {
                var path = GetPath();
                return path.Substring(path.LastIndexOf('/') +1 );
            }

            public string GetPath() {
                return _blob.Uri.ToString().Substring(_rootPath.Length).Trim('/');
            }

            public long GetSize() {       
                return GetDirectorySize(_blob);
            }

            public DateTime GetLastUpdated() {
                return DateTime.MinValue; // TODO this is a bad solution
            }

            public IStorageFolder GetParent() {
                if (_blob.Parent != null) {
                    return new AzureBlobFolderStorage(_blob.Parent, _rootPath);
                }
                throw new ArgumentException("Directory " + _blob.Uri + " does not have a parent directory");
            }

            private static long GetDirectorySize(CloudBlobDirectory directoryBlob) {
                long size = 0;

                foreach ( var blobItem in directoryBlob.ListBlobs() ) {
                    var blob = blobItem as CloudBlockBlob;
                    if (blob != null)
                        size += blob.Properties.Length;

                    var item = blobItem as CloudBlobDirectory;
                    if (item != null)
                        size += GetDirectorySize(item);
                }

                return size;
            }
        }

		public bool TrySaveStream(string path, Stream inputStream)
		{
			try
			{
				SaveStream(path, inputStream);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public void SaveStream(string path, Stream inputStream, DateTime? lastWriteTime = null)
		{
			// Create the file.
			// The CreateFile method will map the still relative path
			var file = CreateFile(path, lastWriteTime);

			using (var outputStream = file.OpenWrite())
			{
				var buffer = new byte[8192];
				for (;;)
				{
					var length = inputStream.Read(buffer, 0, buffer.Length);
					if (length <= 0)
						break;
					outputStream.Write(buffer, 0, length);
				}
			}
		}
    }
}
