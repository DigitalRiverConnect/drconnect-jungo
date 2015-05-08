using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using N2.Azure.Replication;
using N2.Configuration;
using N2.Definitions;
using N2.Definitions.Static;
using N2.Edit.FileSystem;
using N2.Persistence;
using N2.Persistence.Behaviors;
using N2.Persistence.Proxying;
using N2.Persistence.Xml;
using N2.Tests;
using N2.Tests.Fakes;
using NUnit.Framework;

namespace N2.Azure.Tests.Replication
{
    public abstract class XmlRepositoryTestsBase : ItemTestsBase
    {
        protected ContentActivator activator;
        protected IDefinitionManager definitions;
        protected IItemNotifier notifier;
        protected InterceptingProxyFactory proxyFactory;
        protected Type[] persistedTypes = new[] { 
            typeof(TestItem),
            typeof(ListPart),
            typeof(ListItem)};

        protected IFileSystem fs;
        protected IFileSystemFactory fsf;
        protected IContentItemRepository repository;

        [TestFixtureSetUp]
        public virtual void TestFixtureSetup()
        {
            IDefinitionProvider[] definitionProviders;
            fs = new FakeMemoryFileSystem();
            TestSupport.Setup(out definitionProviders, out definitions, out activator, out notifier, out proxyFactory, persistedTypes);

            //var definitionProviders = TestSupport.SetupDefinitionProviders(new DefinitionMap(), typeof(PersistableItem), typeof(NonVirtualItem), typeof(PersistablePart));
            var proxies = new InterceptingProxyFactory();
            proxies.Initialize(definitionProviders.SelectMany(dp => dp.GetDefinitions()));
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            repository = new XmlContentItemRepository(activator, fs, definitions, null, null);
        }
    }

    public class XmlPersisterTestsBase : XmlRepositoryTestsBase
    {
        protected IPersister persister;
        protected Random r;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            r = new Random();
            // compare TestSupport
            var source = TestSupport.SetupContentSource(repository);
            persister = new ContentPersister(source, repository);
            new BehaviorInvoker(persister, new DefinitionMap()).Start();
        }

        [TearDown]
        public override void TearDown()
        {
            persister.Dispose();
            base.TearDown();
        }
    }


    [TestFixture]
    public class XmlReplicationTests : XmlPersisterTestsBase
    {
        protected IReplicationStorage Storage;
        protected ReplicationManager Master;
        protected ReplicationManager Slave;
        protected IContentItemRepository SlaveRepo;
        protected IPersister SlavePersister;
        protected FakeFlushable MasterFlushable;
        protected FakeFlushable SlaveFlushable;
        
        #region Test Data

        // some test items
        private ContentItem item1;
        private ContentItem item1a;
        private ContentItem item1b;
        private ContentItem item2;
        private ContentItem root;
        private ListPart list;

        private long CreateContent()
        {
            // assure storage is empty
            Assert.AreEqual(0, Storage.GetItems().Count());

            // create some content
            root = CreateOneItem<TestItem>(1, "root", null);
            item1 = CreateOneItem<TestItem>(11, "item1", root);
            item2 = CreateOneItem<TestItem>(12, "item2", root);
            item1a = CreateOneItem<TestItem>(111, "item1a", item1);
            item1b = CreateOneItem<TestItem>(112, "item1b", item1);
            repository.SaveOrUpdate(root, item1, item1a, item1b, item2);
            return repository.Count();
        }

        private long CreateContentLarge()
        {
            // assure storage is empty
            Assert.AreEqual(0, Storage.GetItems().Count());

            var createdItems = new List<ContentItem>();

            // create some content
            root = CreateOneItem<TestItem>(1, "root", null);

            createdItems.Add(root);
            repository.SaveOrUpdate(root);

            for (int i = 2; i < 2001; i++)
            {
                var parentIndex = r.Next(i - 1);
                var newItem = CreateOneItem<TestItem>(i, string.Format("item-{0}", i), createdItems[parentIndex]);
                createdItems.Add(newItem);
                repository.SaveOrUpdate(newItem);
            }

            return repository.Count();
        }

        protected ListItem CreateOneChild(int id, string name, ListPart parent)
        {
            ListItem item = (ListItem)Activator.CreateInstance(typeof(ListItem), true);
            item.ID = id;
            item.Name = name;
            item.Title = name;
            item.AncestralTrail = N2.Utility.GetTrail(parent);
            item.SortOrder = parent.Children.Count;
            item.LinkText = "Test_" + name;
            item.Title = "Title_" + name;
            item.AddTo(parent);
            return item;
        }

        private long CreateContent2()
        {
            // assure storage is empty
            Assert.AreEqual(0, Storage.GetItems().Count());

            // create some content
            root = CreateOneItem<TestItem>(1, "root", null);
            item1 = CreateOneItem<TestItem>(11, "item1", root);
            item2 = CreateOneItem<TestItem>(12, "item2", root);
            list = CreateOneItem<ListPart>(1000, "listitem", item1);
            CreateOneChild(91, "listitem1", list);
            CreateOneChild(92, "listitem2", list);
            CreateOneChild(93, "listitem3", list);
            item1a = CreateOneItem<TestItem>(111, "item1a", item1);
            item1b = CreateOneItem<TestItem>(112, "item1b", item1);

            repository.SaveOrUpdate(root, item1, item1a, item1b, item2, list);
            return 5; // # of pages 
        }
        
        #endregion
       
        // see app.config - should use dev storage by default
        protected IFileSystemFactory CreateFileSystemFactory()
        {
            var connectionStrings = (ConnectionStringsSection)ConfigurationManager.GetSection("connectionStrings");
            return new FileSystemFactory(connectionStrings);
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // create a filesystem for replication and use it as storage
            fsf = CreateFileSystemFactory();
            Storage = new ReplicationStorageFileSystemN2(activator, fsf, definitions, persister);

            // create a master replicator
            MasterFlushable = new FakeFlushable();
            Master = new ReplicationManager(persister, new XmlFinder(repository), Storage, fsf, 
                new DatabaseSection(), 
                new FakeSecurityManager(), 
                null,
                MasterFlushable);
            Master.SetUnitTestMode(true);

            // create a separate slave repository
            SlaveRepo = new XmlContentItemRepository(activator, fs, definitions, null, null);
            var slavesource = TestSupport.SetupContentSource(SlaveRepo);
            SlavePersister = new ContentPersister(slavesource, SlaveRepo);
            new BehaviorInvoker(SlavePersister, new DefinitionMap()).Start();

            // create a slave replicator
            SlaveFlushable = new FakeFlushable();
            Slave = new ReplicationManager(SlavePersister, new XmlFinder(SlaveRepo), Storage, fsf,
                new DatabaseSection(),
                new FakeSecurityManager(),
                null,
                SlaveFlushable);
            Slave.SetUnitTestMode(false);
        }

        [Test]
        public void ReplicatedItem_CanParseID()
        {
            var fd = new FileData();
            fd.Name = "page_1234_666.xml";
            fd.Updated = DateTime.Now;
            var ri = new ReplicatedItem(fd);
            Assert.AreEqual(1234, ri.ID);
        }

        [Test]
        public void ReplicatedItem_CanCompareDates()
        {
            var fd = new FileData();
            fd.Name = "1";
            fd.Updated = DateTime.Now;
            var ri = new ReplicatedItem(fd);
            Assert.That(ri.PublishedDateEquals(fd.Updated));
            Assert.That(ri.PublishedDateEquals(fd.Updated.ToUniversalTime()));
            Assert.That(ri.IsOlderThan(DateTime.Now.AddSeconds(1)));
            Assert.That(ri.IsNewerThan(DateTime.Now.AddMinutes(-1)));
        }

        [Test]
        public void ReplicationJournal()
        {
            var j = new ReplicationManager.ReplicationJournal();
            j.LogItem("Test {0} -> {1}", 0, 1);
            j.Log("Null");
            j.LogItem("Two");
            Assert.AreEqual(2, j.AffectedCount);
            Assert.AreEqual("Test 0 -> 1\r\nNull\r\nTwo", j.ToString());
        }

        [Test]
        public void Replication_Locks()
        {
            var writeLock = new ReplicationWriteLockManager(fs);
            var writeLock2 = new ReplicationWriteLockManager(fs);
            var readLock = new ReplicationReadLockManager(fs);
            var readLock2 = new ReplicationReadLockManager(fs);

            Assert.IsFalse(writeLock.IsLocked);
            Assert.IsFalse(writeLock2.IsLocked);
            
            Assert.IsTrue(writeLock.Lock());
            // cannot work as it ignores same name Assert.IsTrue(writeLock.IsLocked);
            //Assert.IsFalse(writeLock2.Lock()); // 2nd lock must fail

            writeLock.Unlock();
            Assert.IsFalse(writeLock.IsLocked);
        }

        [Test]
        public void RepStorage_CanWriteAndRead()
        {
            Assert.AreEqual(0, Storage.GetItems().Count());

            // create an item and export
            ContentItem root = CreateOneItem<TestItem>(0, "root", null);
            Storage.ExportItem(root);

            // check export worked
            Assert.AreEqual(1, Storage.GetItems().Count());
            var ri = Storage.GetItems().Single();
            Assert.AreEqual(root.ID, ri.ID);

            // check timestamps are accurate
            Assert.That(ri.PublishedDateEquals(root.Published.Value));

            // check import works
            var import = Storage.SyncItem(ri);
            Assert.AreEqual(0, import.Attachments.Count);
            Assert.AreEqual(0, import.Errors.Count);
            Assert.AreEqual(0, import.FailedAttachments.Count);
            Assert.AreEqual(0, import.UnresolvedLinks.Count);
            Assert.AreEqual(1, import.ReadItems.Count);
            //     Assert.That(root == import.RootItem); // use overloaded equality
       //     Assert.That(import.RootItem == import.ReadItems.Single()); // use overloaded equality

            // check delete works
            Storage.DeleteItem(ri);
            Assert.AreEqual(0, Storage.GetItems().Count());

            Storage.ExportItem(root);
            var files = Storage.GetItems();
            Assert.AreEqual(1, files.Count());
            
            // check delete by ID works
            Storage.DeleteItem(root.ID);
            Assert.AreEqual(0, Storage.GetItems().Count());
        }

        #region TODO

        // TEST Locking classes & szenarios

        // TEST Date handling

        // TEST Unresolvable links

        // TEST Cascading actions

        // TEST Item sort order changed

        // TEST Item moved to other parent

        // TEST Item moved to Trash

        // TEST Exclusion of Virtual items, especially user data and Trash

        // TEST Corrupted data gets healed (HASHING)

        // TEST IDs are kept on Slave if possible (why would they break?)

        // TEST Child State ?

        #endregion

        [Test]
        public void Replication_Works()
        {
            var count = CreateContent();
            Assert.AreEqual(0, Storage.GetItems().Count());

            // master export all
            Master.Syncronize();
            Assert.AreEqual(count, Storage.GetItems().Count());

            // slave import all, master unaffected
            Slave.Syncronize();
            Assert.AreEqual(count, SlaveRepo.Count());
            Assert.AreEqual(count, repository.Count());
            Assert.IsTrue(SlaveFlushable.Flushed);

            // delete one item
            repository.Delete(item2);
            Assert.IsFalse((repository as XmlContentItemRepository).ExistsLocal(item2)); // ensure file is deleted on disk, too
            Assert.AreEqual(--count, repository.Count());
            Master.Syncronize();
            Assert.AreEqual(count, Storage.GetItems().Count());

            Slave.Syncronize();
            Assert.AreEqual(count, SlaveRepo.Count());
            Assert.IsNull(SlaveRepo.Get(item2.ID));
            Assert.IsFalse((repository as XmlContentItemRepository).ExistsLocal(item2)); // ensure file is deleted on disk, too

            // changing an item does nothing ...
            item1b.Title = "Hello World";
            Assert.AreEqual(0, Master.Syncronize());

            // TODO

            // now publish the item by bumping the date
            item1b.SavedBy = "test";
            item1b.Published = Utility.CurrentTime();
            persister.Save(item1b);

            // create a new item
            ContentItem item3 = CreateOneItem<TestItem>(13, "item3", root);
            item3.SavedBy = "nobody";
            repository.SaveOrUpdate(item3);
            Assert.AreEqual(2, Master.Syncronize());

            Slave.Syncronize();
            Assert.AreEqual(++count, SlaveRepo.Count());
            var i3 = SlaveRepo.Get(item3.ID);
            Assert.NotNull(i3);
            Assert.That(i3 == item3); // TODO verify this check         

            // now update the root
            root.Title = "new root";
            root.Published = Utility.CurrentTime();
            persister.Save(root);
            Master.Syncronize();
            Slave.Syncronize();
            var sroot = SlaveRepo.Get(root.ID);
            Assert.AreEqual(root.Title, sroot.Title);
            Assert.AreEqual(root.Children.Count, sroot.Children.Count);

            // now update item below the root and move it down
            Assert.AreEqual(root, item1.Parent);
            item1.Title = "item1 moved down";
            item1.SortOrder += 99;
            item1.Published = Utility.CurrentTime();
            persister.Save(item1);
            Master.Syncronize();
            Slave.Syncronize();
            var sitem1 = SlaveRepo.Get(item1.ID);
            Assert.AreEqual(item1.Title, sitem1.Title);
            sroot = SlaveRepo.Get(root.ID);
            Assert.AreEqual(root.Children.Count, sroot.Children.Count);
            var sitem3 = SlaveRepo.Get(item3.ID);
            Assert.AreEqual(sitem3, sroot.Children[0]);
            Assert.AreEqual(sitem1, sroot.Children[1]);

            // Master never gets flushed
            Assert.IsFalse(MasterFlushable.Flushed);
        }

        [Test]
        public void Replication_WorksWithChildren()
        {
            var count = CreateContent2();
            Assert.AreEqual(0, Storage.GetItems().Count());

            // master export all
            Master.Syncronize();
            Assert.AreEqual(count, Storage.GetItems().Count());

            // slave import all, master unaffected
            Slave.Syncronize();
            Assert.GreaterOrEqual(SlaveRepo.Count(), count);
            // Assert.AreEqual(count, repository. Count());
            Assert.IsTrue(SlaveFlushable.Flushed);

            var slist = SlaveRepo.Get(list.ID);
            Assert.NotNull(slist);
            Assert.AreEqual(3, slist.Parent.Children.Count); 
            Assert.AreEqual(3, slist.Children.Count);
            Assert.That(slist == list); 

            // delete one item
            list.Children.RemoveAt(1);
            list.Parent.Published = Utility.CurrentTime(); // force sync by republishing owner
            persister.Save(list);

            Assert.AreEqual(1, Master.Syncronize());            
            Slave.Syncronize();

            slist = SlaveRepo.Get(list.ID);
            Assert.NotNull(slist);
            Assert.AreEqual(3, slist.Parent.Children.Count);
            Assert.AreEqual(2, slist.Children.Count);
            Assert.That(slist == list);   
 
            // Master never gets flushed
            Assert.IsFalse(MasterFlushable.Flushed);
        }

        [Test]
        public void Replication_RecoversFromMissingInnerNode()
        {
            var count = CreateContent2();
            Assert.AreEqual(0, Storage.GetItems().Count());

            // master export all
            Master.Syncronize();
            Assert.AreEqual(count, Storage.GetItems().Count());

            // slave import all, master unaffected
            Slave.Syncronize();
            Assert.GreaterOrEqual(SlaveRepo.Count(), count);
            // Assert.AreEqual(count, repository. Count());
            Assert.IsTrue(SlaveFlushable.Flushed);

            var si1 = SlaveRepo.Get(item1.ID);
            Assert.NotNull(si1);

            // DELETE an inner node directly from Storage
            Storage.DeleteItem(item1.ID);
            // will disconnect graph on Slave and result in unresolved links!
            Assert.AreEqual(1, Slave.Syncronize()); // will delete item1 locally 
            si1 = SlaveRepo.Get(item1.ID);
            Assert.IsNull(si1);

            // Another cycle should fix it
            Assert.AreEqual(1, Master.Syncronize()); // deleted item is re-exported

            Assert.AreEqual(3, Slave.Syncronize()); // and properly imported including the 2 children

            si1 = SlaveRepo.Get(item1.ID);
            Assert.NotNull(si1);
            Assert.AreEqual(2, si1.Children.Count(i => i.IsPage)); 
 
            // Master never gets flushed
            Assert.IsFalse(MasterFlushable.Flushed);
        }

        [Test]
        public void Repository_ThreadingWorks()
        {
            var count = CreateContentLarge();

            var readTask = Task.Factory.StartNew(() =>
                {
                    Parallel.For(0, 1000, (x) =>
                        {
                            var items = ((XmlContentItemRepository)repository).AllOfType<TestItem>();
                        });
                });

            var writeTask = Task.Factory.StartNew(() =>
                {
                    int x = 0;
                    while (x++ < 1000)
                    {
                        var id = x++ + 5000;
                        var item = CreateOneItem<TestItem>(id, string.Format("item-{0}", id), root);
                        repository.SaveOrUpdate(item);
                        id = x++ + 5000;
                        item = CreateOneItem<TestItem>(id, string.Format("item-{0}", id), root);
                        repository.SaveOrUpdate(item);
                        id = x++ + 5000;
                        item = CreateOneItem<TestItem>(id, string.Format("item-{0}", id), root);
                        repository.SaveOrUpdate(item);
                        id = x++ + 5000;
                        item = CreateOneItem<TestItem>(id, string.Format("item-{0}", id), root);
                        repository.SaveOrUpdate(item);
                        id = x++ + 5000;
                        item = CreateOneItem<TestItem>(id, string.Format("item-{0}", id), root);
                        repository.SaveOrUpdate(item);
                        Thread.Sleep(10);
                    }
                });

            try
            {
                Task.WaitAll(readTask, writeTask);
            }
            catch (AggregateException)
            {
                Assert.Fail("Parallel tasks failed.");
            }
        }
    }
}
