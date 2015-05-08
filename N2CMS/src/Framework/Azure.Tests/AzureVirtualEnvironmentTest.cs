using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Microsoft.WindowsAzure.Storage;

namespace N2.Azure.Tests {
    public abstract class AzureVirtualEnvironmentTest {
        private Process _dsService;

        private const string AzureSDK =
            "c:\\Program Files\\Microsoft SDKs\\Windows Azure\\";

        protected abstract void OnInit();

        [TestFixtureSetUp]
        public void FixtureSetup() {
            // Orchard-style config 
            var sdk = ConfigurationManager.AppSettings["AzureSDK"] ?? AzureSDK;

            // check if emulator is running
            var count = Process.GetProcessesByName("DSService").Length + Process.GetProcessesByName("DSServiceLDB").Length;
            if ( count == 0 ) {
                // start emulator
                var start = new ProcessStartInfo {
                    Arguments = "/devstore:start",
                    FileName = Path.Combine(sdk, @"emulator\csrun.exe")
                };

                _dsService = new Process { StartInfo = start };
                _dsService.Start();
                _dsService.WaitForExit();
            }

            OnInit();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {

            if ( _dsService != null )
                _dsService.Close();
        }

        protected void DeleteAllBlobs(string containerName, CloudStorageAccount account)
        {
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            container.DeleteAllBlobs();
        }
    }
}
