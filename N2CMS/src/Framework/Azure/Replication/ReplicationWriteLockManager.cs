using System;
using N2.Edit.FileSystem;
using N2.Persistence.Serialization;

namespace N2.Azure.Replication
{
    public class ReplicationWriteLockManager : ReplicationLockManagerBase
    {
        protected const string LockFileName = "_lock_";

        public ReplicationWriteLockManager(IFileSystem fs)
            : base(fs)
        {
        }

        protected override string GenerateLockFullPath()
        {
            return LockPath.TrimEnd('/') + '/' + LockFileName; // don't use Path.Combine
        }

        public override bool IsLocked
        {
            get
            {
                var exists = LockFileExists;
                if (exists)
                {
                    var lockContents = ReadLockContents();
                    if (lockContents.Equals(SerializationUtility.GetLocalhostFqdn(),
                                            StringComparison.InvariantCultureIgnoreCase))
                        return false;

                    var file = _fs.GetFile(_lockFullPath);
                    if (file != null && file.Created.AddMilliseconds(_timerInterval) < DateTime.Now)
                    {
                        _logger.ErrorFormat("{0} - Lock file was left locked after the lock interval.", lockContents);
                        Unlock();
                        return false;
                    }
                }

                return exists;
            }
        }

        protected override bool CanLock
        {
            get
            {
                if (IsLocked)
                {
                    _logger.Warn("Cannot lock replication because it is already locked.");
                    return false;
                }

                return true;
            }
        }
    }
}