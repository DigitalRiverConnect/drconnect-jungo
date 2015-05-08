using System;
using System.Configuration;
using N2.Engine;
using N2.Edit.FileSystem;

namespace N2.Azure
{
    [Service(typeof(IFileSystemFactory))]
    public class FileSystemFactory : IFileSystemFactory
    {
        private readonly ConnectionStringsSection _css;
        public FileSystemFactory(ConnectionStringsSection connectionStrings)
        {
            _css = connectionStrings;
        }

        /// <summary>
        /// Creates an instance of IFileSystem for a namespace other than the default.
        /// </summary>
        /// <param name="namespaceName">The name of the namespace upon which the file system operates</param>
        /// <returns></returns>
        public IFileSystem Create(FileSystemNamespace namespaceName)
        {
            IFileSystem fs = null;

            // TODO refactor this ugly pattern
            if (namespaceName == FileSystemNamespace.CSS)
                fs = new AzureStorageFileSystem(_css, "css", false);
            else if (namespaceName == FileSystemNamespace.JavaScript)
                fs = new AzureStorageFileSystem(_css, "javascript", false);
            else if (namespaceName == FileSystemNamespace.ReplicationStorageDebug)
                fs = new AzureStorageFileSystem(_css, "replication-debug", true);
            else if (namespaceName == FileSystemNamespace.ReplicationStorageE1)
                fs = new AzureStorageFileSystem(_css, "replication-e1", true);
            else if (namespaceName == FileSystemNamespace.ReplicationStorageE2)
                fs = new AzureStorageFileSystem(_css, "replication-e2", true);

            return fs;
        }
    }
}
