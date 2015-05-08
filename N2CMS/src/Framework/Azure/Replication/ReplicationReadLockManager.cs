using System;
using System.IO;
using System.Linq;
using N2.Edit.FileSystem;
using N2.Persistence.Serialization;

namespace N2.Azure.Replication
{
    public class ReplicationReadLockManager : ReplicationLockManagerBase
    {
        private const string LockFileNamePrefix = "_read_lock_";

        public ReplicationReadLockManager(IFileSystem fs)
            : base(fs)
        {
        }

        public override bool IsLocked
        {
            get
            {
                var files = _fs.GetFiles(LockPath).ToList();
                return files.Any(f => f.Name.StartsWith(LockFileNamePrefix) && f.Created.AddMilliseconds(_timerInterval) > DateTime.Now);
            }
        }

        protected override bool CanLock
        {
            // Can always lock
            get { return true; }
        }

        protected override string GenerateLockFullPath()
        {
            return Path.Combine(LockPath, LockFileNamePrefix + SerializationUtility.GetLocalhostFqdn());
        }
    }
}