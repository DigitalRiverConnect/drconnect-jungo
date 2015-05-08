using System;
using log4net;
using log4net.Core;

namespace Jungo.Infrastructure.Logger
{
    public class TraceLogger : ITraceLogger
    {
        private readonly ILog _log;

        public TraceLogger()
        {
            _log = LogManager.GetLogger(typeof(TraceLogger));
        }

        #region Implementation of ILogger

        public void Debug(string format, params object[] args)
        {
            if (!_log.IsDebugEnabled) return;
            _log.DebugFormat(format, args);
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            if (!_log.IsDebugEnabled) return;
            _log.Debug(string.Format(format, args), exception);
        }

        public void Info(string format, params object[] args)
        {
            if (!_log.IsInfoEnabled) return;
            _log.InfoFormat(format, args);
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            if (!_log.IsInfoEnabled) return;
            _log.Info(string.Format(format, args), exception);
        }

        public void Trace(string format, params object[] args)
        {
            if (!_log.IsInfoEnabled) return;
            _log.Logger.Log(typeof(TraceLogger), Level.Trace, String.Format(format, args), null);
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
            if (!_log.IsInfoEnabled) return;
            _log.Logger.Log(typeof(TraceLogger), Level.Trace, String.Format(format, args), exception);
        }

        public void Warn(string format, params object[] args)
        {
            if (!_log.IsWarnEnabled) return;
            _log.WarnFormat(format, args);
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            if (!_log.IsWarnEnabled) return;
            _log.Warn(string.Format(format, args), exception);
        }



        public void Error(string format, params object[] args)
        {
            if (!_log.IsErrorEnabled) return;
            _log.ErrorFormat(format, args);
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            if (!_log.IsErrorEnabled) return;
            _log.Error(string.Format(format, args), exception);
        }

        public void Fatal(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            _log.Fatal(string.Format(format, args), exception);
        }

        #endregion

        #region private parts

        private bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        private bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled; }
        }

        private bool IsTraceEnabled
        {
            get { return _log.Logger.IsEnabledFor(Level.Trace); }
        }

        private bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled; }
        }

        private bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled; }
        }

        private bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled; }
        }

        #endregion
    }
}
