using System;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace N2.Azure.Tests {
    [TestFixture]
    public class AzureShellSettingsManagerTests : AzureVirtualEnvironmentTest {

        protected CloudStorageAccount DevAccount;
        private const String ContainerName = "n2files";

        protected override void OnInit() {
            CloudStorageAccount.TryParse("UseDevelopmentStorage=true", out DevAccount);
        }

        [SetUp]
        public void Setup() {
            // ensure default container is empty before running any test
            DeleteAllBlobs(ContainerName, DevAccount);
        }

        [TearDown]
        public void TearDown() {
            // ensure default container is empty after running tests
            DeleteAllBlobs(ContainerName, DevAccount);
        }
    }
}
