using System;
using System.Configuration;
using System.IO;
using System.Timers;
using N2.Edit.FileSystem;
using N2.Engine;
using N2.Persistence.Serialization;

namespace N2.Azure.Replication
{
    public abstract class ReplicationLockManagerBase : IDisposable
    {
        public const string DefaultTimerInterval = "600000";
        protected const string LockPath = "/_Xml_Sync_Lock_";
        protected readonly string _lockFullPath;
        protected readonly IFileSystem _fs;
        protected readonly Logger<IReplicationStorage> _logger;
        protected readonly int _timerInterval;
        protected readonly Timer _timer;

        public abstract bool IsLocked { get; }
        protected abstract bool CanLock { get; }

        protected ReplicationLockManagerBase(IFileSystem fs)
        {
            _timerInterval = int.Parse(ConfigurationManager.AppSettings["ReplicationMaxLockDuration"] ?? DefaultTimerInterval);
            _timer = new Timer(_timerInterval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
            _lockFullPath = GenerateLockFullPath();
            _fs = fs;
        }

        protected abstract string GenerateLockFullPath();

        protected bool LockFileExists
        {
            get { return _fs.FileExists(_lockFullPath); }
        }

        public bool Lock(int? timerIntervalOverride = null)
        {
            if (!CanLock)
                return false;

            try
            {
                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                {
                    _timer.Stop();

                    if (timerIntervalOverride.HasValue)
                        _timer.Interval = (double) timerIntervalOverride;
                    else
                        _timer.Interval = _timerInterval;

                    sw.Write(GenerateLockFileContents());
                    sw.Flush();
                    ms.Position = 0;

                    _fs.WriteFile(_lockFullPath, ms, GenerateCreatDateTime());

                    // Unlock after a configured interval
                    _timer.Start();

                    _logger.Info("Locked replication");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to lock replication. " + ex.Message);
                return false;
            }
        }

        public void Unlock()
        {
            if (LockFileExists)
            {
                _fs.DeleteFile(_lockFullPath);
                _logger.Info("Unlocked replication");
            }

            if (LockFileExists)
            {
                _logger.ErrorFormat("Unable to remove lock file from {0}.", _lockFullPath);
            }
        }

        protected virtual DateTime GenerateCreatDateTime()
        {
            return DateTime.UtcNow;
        }

        protected virtual string GenerateLockFileContents()
        {
            return SerializationUtility.GetLocalhostFqdn();
        }

        protected void OnTimerElapsed(object source, ElapsedEventArgs e)
        {
            if (LockFileExists)
            {
                _logger.Error("Time elapsed for replication lock. Forcing it to unlock.");
                Unlock();
                _timer.Stop();
            }
        }

        protected string ReadLockContents()
        {
            try
            {

                using (var fs = _fs.OpenFile(_lockFullPath, true))
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
                _logger.Error("Unable to read lock file.");
                return "";
            }
        }


        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}