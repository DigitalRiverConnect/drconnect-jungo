using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using N2.Edit;
using N2.Edit.FileSystem;
using N2.Engine;
using N2.Resources;

// adapts IFileSystem to Orchard's implementation
// by sweber@digitalriver.com
// should become OpenSource

namespace N2.Azure
{
    [Service(typeof(IFileSystem), Configuration = "AzureFS", Replaces = typeof(MappedFileSystem))]
    public class AzureStorageFileSystem : IFileSystem, IWebAccessible
    {
        private readonly AzureFileSystem isp;
        private string _root;

        public AzureStorageFileSystem(ConnectionStringsSection connectionStrings)
        {
            isp = Init(connectionStrings, null);
        }

        public AzureStorageFileSystem(ConnectionStringsSection connectionStrings, string containerName, bool isPrivate)
        {
            isp = Init(connectionStrings, containerName, isPrivate);
        }

        private AzureFileSystem Init(ConnectionStringsSection connectionStrings, string containerName, bool isPrivate = false)
        {
            ConnectionStringSettings css = connectionStrings.ConnectionStrings["AzureStorageConnection"];
            string connectionString = css != null
                                   ? css.ConnectionString
                                   : (ConfigurationManager.AppSettings["AzureStorageConnectionString"] ?? "UseDevelopmentStorage=true");
            var account = CloudStorageAccount.Parse(connectionString);
            if (string.IsNullOrEmpty(containerName))
                containerName = ConfigurationManager.AppSettings["AzureStorageContainerName"] ?? "n2tests";
            _root = ConfigurationManager.AppSettings["AzureStorageRootFolderName"] ?? "default"; // orchard: default
            string delAll = ConfigurationManager.AppSettings["AzureStorageDeleteAllOnStartup"] ?? "false";

            if (account == null) 
                throw new ConfigurationErrorsException("Bad Azure Storage Configuration");
            
            var isp = new AzureFileSystem(containerName, _root, isPrivate, account);
            if (delAll.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                isp.Container.DeleteAllBlobs(); // start with a fresh container, used in unit tests
            }
            return isp;
        }

        #region private methods

        private static FileData GetFileData(IStorageFile file)
        {
            if (file == null) return null;

            var lastUpdate = file.GetLastUpdated();
            return new FileData
            {
                Name = file.GetName(),
                VirtualPath = "/" + file.GetPath(),
                Created = lastUpdate,
                Updated = lastUpdate,
                Length = file.GetSize()
            };
        }

        private DirectoryData GetDirectoryData(IStorageFolder folder)
        {
            if (folder == null) return null;

            // TODO find a better way to get the folder's date
            // var folderEntry = isp.GetFile(folder.GetPath() + '/' + AzureFileSystem.FolderEntry);

            return new DirectoryData
                {
                    Name = folder.GetName(),
                    VirtualPath = "/" + folder.GetPath(),
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    //Created = folderEntry.GetLastUpdated(),
                    //Updated = folderEntry.GetLastUpdated()
                };

        }

        private static string ToRelative(string path)
        {
            path = path.TrimStart('~');
            return path.StartsWith("/") ? path.Substring(1) : path;
        }

        #endregion

        #region IFileSystem members

        public IEnumerable<FileData> GetFiles(string parentVirtualPath)
        {
            return isp.ListFiles(ToRelative(parentVirtualPath)).Select(GetFileData);
        }

        public FileData GetFile(string virtualPath)
        {
            var path = ToRelative(virtualPath);
            return (isp.FileExists((path))) ? GetFileData(isp.GetFile(path)) : null;
        }

        public IEnumerable<DirectoryData> GetDirectories(string parentVirtualPath)
        {
            return isp.ListFolders(ToRelative(parentVirtualPath)).Select(GetDirectoryData);
        }

        public DirectoryData GetDirectory(string virtualPath)
        {
            var path = ToRelative(virtualPath).TrimEnd('/');
            if (DirectoryExists(path))
            {
                var lastSlash = path.LastIndexOf('/') + 1;
                var dirpath = path.Substring(0, lastSlash);
                var dirname = path.Substring(lastSlash);

                var dir = GetDirectories(dirpath);
                return (dir != null) ? dir.FirstOrDefault(e => e.Name.Equals(dirname)) : null;
            }
            return null;
        }

        public bool FileExists(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return false;

            return isp.FileExists(ToRelative(virtualPath)); // TODO not in IStorageProvider
        }

        public void MoveFile(string fromVirtualPath, string destinationVirtualPath)
        {
            // TODO: check if this works for move
            isp.RenameFile(ToRelative(fromVirtualPath), ToRelative(destinationVirtualPath));

            if (FileMoved != null)
                FileMoved.Invoke(this, new FileEventArgs(destinationVirtualPath, fromVirtualPath));
        }

        public void DeleteFile(string virtualPath)
        {
            isp.DeleteFile(ToRelative(virtualPath));

            if (FileDeleted != null)
                FileDeleted.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        public void CopyFile(string fromVirtualPath, string destinationVirtualPath)
        {
            var source = isp.GetFile(ToRelative(fromVirtualPath));

            WriteFile(destinationVirtualPath, source.OpenRead());

            if (FileCopied != null)
                FileCopied.Invoke(this, new FileEventArgs(destinationVirtualPath, fromVirtualPath));
        }

        public Stream OpenFile(string virtualPath, bool readOnly = false)
        {
            var file = isp.GetFile(ToRelative(virtualPath));
            if (file == null) return null;

            return readOnly ? file.OpenRead() : file.OpenWrite();
        }

        public void WriteFile(string virtualPath, Stream inputStream, DateTime? lastWriteTime = null)
        {
            isp.SaveStream(ToRelative(virtualPath), inputStream, lastWriteTime);

            if (FileWritten != null)
                FileWritten.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        /// <summary>Read file contents to a stream.</summary>
        /// <param name="virtualPath">The path of the file to read.</param>
        /// <param name="outputStream">The stream to which the file contents should be written.</param>
        public void ReadFileContents(string virtualPath, Stream outputStream)
        {
            using (var sourceFile = OpenFile(ToRelative(virtualPath), true))
            {
                var buffer = new byte[32768];
                while (true)
                {
                    var bytesRead = sourceFile.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0)
                        break;

                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        public bool DirectoryExists(string virtualPath)
        {
            return isp.DirectoryExists(ToRelative(virtualPath));
        }

        public void MoveDirectory(string fromVirtualPath, string destinationVirtualPath)
        {
            isp.RenameFolder(ToRelative(fromVirtualPath), ToRelative(destinationVirtualPath));

            if (DirectoryMoved != null)
                DirectoryMoved.Invoke(this, new FileEventArgs(destinationVirtualPath, fromVirtualPath));
        }

        public void DeleteDirectory(string virtualPath)
        {
            isp.DeleteFolder(ToRelative(virtualPath));

            if (DirectoryDeleted != null)
                DirectoryDeleted.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        public void CreateDirectory(string virtualPath)
        {
            isp.CreateFolder(ToRelative(virtualPath));

            if (DirectoryCreated != null)
                DirectoryCreated.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        public event EventHandler<FileEventArgs> FileWritten;
        public event EventHandler<FileEventArgs> FileCopied;
        public event EventHandler<FileEventArgs> FileMoved;
        public event EventHandler<FileEventArgs> FileDeleted;
        public event EventHandler<FileEventArgs> DirectoryCreated;
        public event EventHandler<FileEventArgs> DirectoryMoved;
        public event EventHandler<FileEventArgs> DirectoryDeleted;

        #endregion

        public string GetPublicURL(string filePath)
        {
            if (isp.Container.GetPermissions().PublicAccess == BlobContainerPublicAccessType.Off)
                throw new AccessViolationException();

            if (filePath == null)
                return isp.Container.Uri.ToString() + '/' + _root;

            return isp.GetPublicUrl(ToRelative(filePath) ?? filePath);
        }
    }
}
