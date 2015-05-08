using System;
using System.Collections.Generic;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests
{
	public class LogEntry
	{
		public Exception Ex;
		public string Format;
		public object[] Args;
	}

	public class FakeLogger : IRequestLogger
	{
		public List<LogEntry> Log = new List<LogEntry>();

		#region ILogger Members

		public Type Type
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsDebugEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsInfoEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsTraceEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsWarnEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsErrorEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsFatalEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public void Debug(string format, params object[] args)
		{
			Log.Add(new LogEntry { Format = format, Args = args });
		}

		public void Debug(Exception exception, string format, params object[] args)
		{
			Log.Add(new LogEntry { Ex = exception, Format = format, Args = args });
		}

		public void Info(string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void Info(Exception exception, string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void Trace(string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void Trace(Exception exception, string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void Warn(string format, params object[] args)
		{
			Log.Add(new LogEntry { Format = format, Args = args });
		}

		public void Warn(Exception exception, string format, params object[] args)
		{
			Log.Add(new LogEntry {Ex = exception, Format = format, Args = args});
		}

		public void Error(string format, params object[] args)
		{
			Log.Add(new LogEntry { Format = format, Args = args });
		}

		public void Error(Exception exception, string format, params object[] args)
		{
			Log.Add(new LogEntry { Ex = exception, Format = format, Args = args });
		}

		public void Fatal(string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void Fatal(Exception exception, string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		#endregion

        public void AddLogRecord(LogRecord logRecord)
        {
            throw new NotImplementedException();
        }

        public JungoLogRecord BeginJungoLog(object callingClass, string memberName = "")
        {
            throw new NotImplementedException();
        }

        public MvcLogRecord MvcLogRecord
        {
            get { throw new NotImplementedException(); }
        }

        public Guid RequestId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string SessionId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void WriteLog()
        {
            throw new NotImplementedException();
        }
    }
}
