using System;
using N2.Edit.FileSystem;

namespace N2.Azure
{
    public enum FileSystemNamespace
    {
        CSS,
        JavaScript,
        ReplicationStorageE1,
        ReplicationStorageE2,
        ReplicationStorageDebug,
    }

    public interface IFileSystemFactory
    {
        IFileSystem Create(FileSystemNamespace namespaceName);
    }
}
