using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N2.Edit.FileSystem;

namespace N2.Azure.Replication
{
    public class ReplicationForceWriteLockManager : ReplicationWriteLockManager
    {
        public ReplicationForceWriteLockManager(IFileSystem fs)
            : base(fs)
        {
        }

        protected override bool CanLock
        {
            get { return true; }
        }

        protected override string GenerateLockFileContents()
        {
            return "force-lock";
        }

        protected override DateTime GenerateCreatDateTime()
        {
            // Make it expire a year from now.
            return DateTime.Now.AddYears(1);
        }
    }
}
