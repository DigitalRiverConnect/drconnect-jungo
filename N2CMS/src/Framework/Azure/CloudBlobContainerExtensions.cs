using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace N2.Azure {
    public static class CloudBlobContainerExtensions {

        public static bool BlobExists(this CloudBlobContainer container, string path) {
            if ( String.IsNullOrEmpty(path) || path.Trim() == String.Empty )
                throw new ArgumentException("Path can't be empty");

            try {
                return container.GetBlockBlobReference(path).Exists();
            }
            catch ( StorageException e ) {
                RequestResult requestInformation = e.RequestInformation;
                if (requestInformation.HttpStatusCode == (int)System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                if (requestInformation.HttpStatusCode == (int)System.Net.HttpStatusCode.Forbidden)
                {
                    return false; // TODO investigate this case
                }

                throw;
            }
        }

        public static void EnsureBlobExists(this CloudBlobContainer container, string path) {
            if ( !BlobExists(container, path) ) {
                throw new ArgumentException("File " + path + " does not exist");
            }
        }

        public static void EnsureBlobDoesNotExist(this CloudBlobContainer container, string path) {
            if ( BlobExists(container, path) ) {
                throw new ArgumentException("File " + path + " already exists");
            }
        }

        public static bool DirectoryExists(this CloudBlobContainer container, string path) {
            if ( String.IsNullOrEmpty(path) || path.Trim() == String.Empty )
                throw new ArgumentException("Path can't be empty");

            return container.GetDirectoryReference(path).ListBlobs().Any();
        }

        public static void EnsureDirectoryExists(this CloudBlobContainer container, string path) {
            if ( !DirectoryExists(container, path) ) {
                throw new ArgumentException("Directory " + path + " does not exist");
            }
        }

        public static void EnsureDirectoryDoesNotExist(this CloudBlobContainer container, string path) {
            if ( DirectoryExists(container, path) ) {
                throw new ArgumentException("Directory " + path + " already exists");
            }
        }

        public static void DeleteAllBlobs(this CloudBlobContainer container)
        {
            foreach (var blob in container.ListBlobs())
            {
                var blockBlob = blob as CloudBlockBlob;
                if (blockBlob != null)
                {
                    blockBlob.DeleteIfExists();
                }

                var directory = blob as CloudBlobDirectory;
                if (directory != null)
                {
                    DeleteAllBlobsDir(directory);                
                }
            }
        }

        private static void DeleteAllBlobsDir(CloudBlobDirectory cloudBlobDirectory)
        {
            foreach (var blob in cloudBlobDirectory.ListBlobs())
            {
                var blockBlob = blob as CloudBlockBlob;
                if (blockBlob != null)
                {
                    blockBlob.DeleteIfExists();
                }

                var directory = blob as CloudBlobDirectory;
                if (directory != null)
                {
                    DeleteAllBlobsDir(directory);
                }
            }
        }
    }
}
