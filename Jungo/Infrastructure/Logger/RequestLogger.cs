using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using Jungo.Infrastructure.Config;
using log4net.Appender;
using log4net.Repository.Hierarchy;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Jungo.Infrastructure.Logger
{
    public interface IRequestLogger : ITraceLogger
    {
        string SessionId { get; set; }
        Guid RequestId { get; set; }
        MvcLogRecord MvcLogRecord { get; }
        JungoLogRecord BeginJungoLog(object callingClass, [CallerMemberName] string memberName = "");
        void AddLogRecord(LogRecord logRecord);
        void WriteLog();
    }

    public class RequestLogger : IRequestLogger
    {
        public const string ItemKey = "_jungorecord_";
        public const string RequestIdKey = "_jungoRequestId_";

        public RequestLogger(bool inRequestScope)
        {
            CheckConfig();
            var context = HttpContext.Current;
            if (context == null || context.Items.Count == 0 || !inRequestScope) return;

            RequestId = Guid.NewGuid();

            AddLogRecord(new MvcLogRecord
            {
                Host = Environment.MachineName,
                Uri = context.Request.Url.ToString(),
                HttpMethod = context.Request.HttpMethod,
            });
        }

        public static void AddRequestLogger(IRequestLogger requestLogger)
        {
            AddRequestLogger(requestLogger.RequestId, requestLogger);
        }

        public static void AddRequestLogger(Guid requestId, IRequestLogger requestLogger)
        {
            _requestLoggers.TryAdd(requestId, requestLogger);
        }

        public static IRequestLogger GetRequestLogger(Guid requestId)
        {
            IRequestLogger requestLogger;
            return _requestLoggers.TryGetValue(requestId, out requestLogger) ? requestLogger : null;
        }

        public static void RemoveRequestLogger(IRequestLogger requestLogger)
        {
            RemoveRequestLogger(requestLogger.RequestId);
        }

        public static void RemoveRequestLogger(Guid requestId)
        {
            IRequestLogger removedLogger;
            _requestLoggers.TryRemove(requestId, out removedLogger);
        }

        public static IRequestLogger Current
        {
            get
            {
                var requestId = CallContext.LogicalGetData(RequestIdKey);
                var requestLogger = requestId != null ? GetRequestLogger((Guid)requestId) : GetRequestLogger(new Guid());
                if (requestLogger != null)
                    return requestLogger;
                if (HttpContext.Current != null && HttpContext.Current.Items[ItemKey] != null)
                    return HttpContext.Current.Items[ItemKey] as IRequestLogger;
                return new RequestLogger(inRequestScope: false);
            }
        }

        #region Implementation of IJungoLogger interface

        public string SessionId { get; set; }
        public Guid RequestId { get; set; }

        public MvcLogRecord MvcLogRecord { get; set; }

        public JungoLogRecord BeginJungoLog(object callingClass, [CallerMemberName]string memberName = "")
        {
            var log = new JungoLogRecord { RequestType = String.Format("{0}.{1}", callingClass.GetType().Name, memberName) };
            AddLogRecord(log);
            return log;
        }

        public void AddLogRecord(LogRecord logRecord)
        {
            logRecord.Order = ++_counter;
            var mvcLogRecord = logRecord as MvcLogRecord;
            if (mvcLogRecord != null)
                MvcLogRecord = mvcLogRecord;
            else
                _logRecords.Add(logRecord);
        }

        public void WriteLog()
        {
            var requestId = RequestId.ToString("n");
            if (MvcLogRecord != null)
            {
                _log.InfoFormat(_logFormat, new object[]
                {
                    FormatDate(MvcLogRecord.DateRequested),
                    MvcLogRecord.Order.ToString(CultureInfo.InvariantCulture),
                    MvcLogRecord.LogType,
                    MvcLogRecord.RequestType,
                    MvcLogRecord.TimeElapsed,
                    MvcLogRecord.Host,
                    MvcLogRecord.Uri,
                    String.Empty, // request payload
                    MvcLogRecord.HttpStatus.ToString(CultureInfo.InvariantCulture),
                    String.Empty, // data source
                    String.Empty, // response payload
                    MvcLogRecord.Error,
                    SessionId,
                    requestId,
                    String.Empty, // dr-request-id
                    String.Empty,
                    String.Empty,
                    MvcLogRecord.ServerTrace == null ? String.Empty : MvcLogRecord.ServerTrace.ToString()
                });
            }
            foreach (var logRecord in _logRecords)
            {
                var jungoLogRecord = logRecord as JungoLogRecord;
                if (jungoLogRecord != null)
                {
                    _log.InfoFormat(_logFormat, new object[]
                    {
                        FormatDate(jungoLogRecord.DateRequested),
                        jungoLogRecord.Order.ToString(CultureInfo.InvariantCulture),
                        JungoLogRecord.LogType,
                        jungoLogRecord.RequestType,
                        jungoLogRecord.TimeElapsed,
                        String.Empty, // host
                        String.Empty, // uri
                        String.Empty, // request payload
                        String.Empty, // response status
                        String.Empty, // data source
                        String.Empty, // response payload
                        jungoLogRecord.Error,
                        SessionId,
                        requestId,
                        String.Empty, // dr-request-id
                        String.Empty,
                        String.Empty,
                        String.Empty
                    });
                }
                var shopperApiLogRecord = logRecord as ShopperApiLogRecord;
                if (shopperApiLogRecord != null)
                {
                    _log.InfoFormat(_logFormat, new object[] {
                        FormatDate(shopperApiLogRecord.DateRequested),
                        shopperApiLogRecord.Order.ToString(CultureInfo.InvariantCulture),
                        ShopperApiLogRecord.LogType,
                        shopperApiLogRecord.RequestType,
                        shopperApiLogRecord.TimeElapsed,
                        shopperApiLogRecord.Host,
                        shopperApiLogRecord.Uri,
                        String.IsNullOrEmpty(shopperApiLogRecord.RequestPayload) ? String.Empty : _configuredDelimiter == JungoLoggerLog4NetDelimiter.Tab ?
                            shopperApiLogRecord.RequestPayload.Replace("\t", " ") :
                            "\"" + shopperApiLogRecord.RequestPayload.Replace("\"", "\"\"") + "\"",
                        shopperApiLogRecord.HttpStatus == 0 ? String.Empty : shopperApiLogRecord.HttpStatus.ToString(CultureInfo.InvariantCulture),
                        shopperApiLogRecord.DataFlow.ToString(),
                        String.IsNullOrEmpty(shopperApiLogRecord.ResponsePayload) ? String.Empty : _configuredDelimiter == JungoLoggerLog4NetDelimiter.Tab ?
                            shopperApiLogRecord.ResponsePayload.Replace("\t", " ") :
                            "\"" + shopperApiLogRecord.ResponsePayload.Replace("\"", "\"\"") + "\"",
                        shopperApiLogRecord.Error,
                        SessionId,
                        requestId,
                        shopperApiLogRecord.DrRequestId,
                        shopperApiLogRecord.ResponsePayloadFirst1000,
                        shopperApiLogRecord.ResponsePayloadCompressed,
                        String.Empty
                    });
                }
            }
        }

        #endregion

        #region Implementation of ITraceLogger interface

        private const string LogLevelDebug =    "DEBUG";
        private const string LogLevelInfo =     "INFO";
        private const string LogLevelTrace =    "TRACE";
        private const string LogLevelWarn =     "WARN";
        private const string LogLevelError =    "ERROR";
        private const string LogLevelFatal =    "FATAL";

        public void Debug(string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelDebug)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelDebug, format, null, args);
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelDebug)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelDebug, format, exception, args);
        }

        public void Info(string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelInfo)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelInfo, format, null, args);
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelInfo)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelInfo, format, exception, args);
        }

        public void Trace(string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelTrace)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelTrace, format, null, args);
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelTrace)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelTrace, format, exception, args);
        }

        public void Warn(string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelWarn)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelWarn, format, null, args);
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelWarn)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelWarn, format, exception, args);
        }

        public void Error(string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelError)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelError, format, null, args);
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelError)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelError, format, exception, args);
        }

        public void Fatal(string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelFatal)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelFatal, format, null, args);
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            if (!_logLevelsIncluded.Contains(LogLevelFatal)) return;
            if (MvcLogRecord == null) return;
            MvcLogRecord.AppendServerTrace(LogLevelFatal, format, exception, args);
        }

        #endregion

        #region Private Parts

        private const int ReconfigInterval = 5; // minutes
        private static DateTime _lastConfig = DateTime.MinValue;
        private static log4net.ILog _log;
        private static string _logFormat;
        private static JungoLoggerLog4NetDelimiter _configuredDelimiter = JungoLoggerLog4NetDelimiter.Tab;
        private static readonly object JungoLock = new object();
        private static string[] _logLevelsIncluded;
        private static ConcurrentDictionary<Guid, IRequestLogger> _requestLoggers = new ConcurrentDictionary<Guid, IRequestLogger>();

        private static void CheckConfig()
        {
            lock (JungoLock)
            {
                if (!String.IsNullOrEmpty(_logFormat) && DateTime.Now.Subtract(_lastConfig).Minutes < ReconfigInterval)
                    return;
                ReconfigureLog();
                _lastConfig = DateTime.Now;
            }
        }

        private static void ReconfigureLog()
        {
            var hier = log4net.LogManager.GetRepository() as Hierarchy;
            if (hier != null)
            {
                // Get appender
                var jungoAppender = (RollingFileAppender)hier.GetAppenders()
                    .FirstOrDefault(
                        appender => appender.Name.Equals("JungoAppender", StringComparison.InvariantCultureIgnoreCase));

                if (jungoAppender != null)
                {
                    var config = ConfigLoader.Get<JungoLoggerLog4NetConfig>();
                    _configuredDelimiter = config.Delimiter;
                    _logLevelsIncluded = config.LogLevelsIncluded;
                    var delimiter = _configuredDelimiter == JungoLoggerLog4NetDelimiter.Comma ? "," : "\t";
                    var header = new StringBuilder("datetime");
                    var logFormatBuilder = new StringBuilder("{0}");
                    foreach (var column in config.Columns)
                    {
                        var colNum = -1;
                        switch (column.FieldName)
                        {
                            case "Order":                       colNum = 1; break;
                            case "LogType":                     colNum = 2; break;
                            case "RequestType":                 colNum = 3; break;
                            case "TimeElapsed":                 colNum = 4; break; 
                            case "Host":                        colNum = 5; break;
                            case "Uri":                         colNum = 6; break;
                            case "RequestPayload":              colNum = 7; break;
                            case "HttpStatus":                  colNum = 8; break;
                            case "DataFlow":                    colNum = 9; break;
                            case "ResponsePayload":             colNum = 10; break;
                            case "Error":                       colNum = 11; break;
                            case "SessionId":                   colNum = 12; break;
                            case "RequestId":                   colNum = 13; break;
                            case "DrRequestId":                 colNum = 14; break;
                            case "ResponsePayloadFirst1000":    colNum = 15; break;
                            case "ResponsePayloadCompressed":   colNum = 16; break;
                            case "ServerTrace":                 colNum = 17; break;
                        }
                        if (colNum < 0) continue;
                        header.Append(String.Format("{0}{1}", delimiter, column.Heading));
                        logFormatBuilder.Append(String.Format("{0}{{{1}}}", delimiter, colNum));
                    }
                    header.Append("\n\r");
                    if (config.Columns.Count() < 17)
                    {
                        for (var i = config.Columns.Count(); i < 17; i++)
                            logFormatBuilder.Append(delimiter);
                    }
                    _logFormat = logFormatBuilder.ToString();
                    var layout = new log4net.Layout.PatternLayout
                    {
                        Header = header.ToString(),
                        ConversionPattern = "%message%newline"
                    };
                    layout.ActivateOptions();
                    jungoAppender.Layout = layout;
                    jungoAppender.ActivateOptions();
                }
            }
            _log = log4net.LogManager.GetLogger("JungoLogger");
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss:fff");
        }

        private int _counter;
        private readonly List<LogRecord> _logRecords = new List<LogRecord>();

        #endregion
    }

    public class MvcLogRecord : HttpLogRecord
    {
        public const string LogType = "Mvc";
    }

    public class JungoLogRecord : LogRecord
    {
        public const string LogType = "JungoApi";
        public int StatusCode { get; set; }
    }

    public enum DataFlow
    {
        FromShopperApi,
        FromShortTermCache,
        FromLongTermCache,
        CacheRefresh
    }

    public class ShopperApiLogRecord : HttpLogRecord
    {
        public const string LogType = "ShopperApi";
        public string DrRequestId { get; set; }
        public DataFlow DataFlow { get; set; }
    }

    public class HttpLogRecord : LogRecord
    {
        public string Host { get; set; }
        public string Uri { get; set; }
        public string HttpMethod { get; set; }
        public int HttpStatus { get; set; }
    }

    public class LogRecord: IDisposable
    {
        public LogRecord()
        {
            DateRequested = DateTime.UtcNow;
        }
        public DateTime DateRequested { get; private set; }
        public DateTime DateResponded { get; set; }
        public string RequestType { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string Error { get; set; }
        public int Order { get; set; }
        public StringBuilder ServerTrace { get; private set; }

        internal void AppendServerTrace(string logLevel, string format, Exception exception = null, params object[] args)
        {
            if (String.IsNullOrEmpty(format) && exception == null)
                return;
            String trace;
            if (!String.IsNullOrEmpty(format) && exception != null)
            {
                var internalFormat = String.Format("; [{0}] {1}\n{2}:{3}\n{4}", logLevel, format,
                    exception.GetType().FullName,
                    exception.Message, exception.StackTrace);
                trace = String.Format(internalFormat, args);
            }
            else if (exception != null)
                trace = String.Format("; [{0}]\n{1}:{2}\n{3}", logLevel, exception.GetType().FullName,
                    exception.Message, exception.StackTrace);
            else
            {
                var internalFormat = String.Format("; [{0}] {1}", logLevel, format);
                trace = String.Format(internalFormat, args);
            }

            if (ServerTrace == null)
            {
                ServerTrace = new StringBuilder();
                trace = trace.Substring(2); // strip of leading "; " for the first trace
            }
            ServerTrace.Append(trace);
        }

        public string ResponsePayloadFirst1000
        {
            get
            {
                if (String.IsNullOrEmpty(ResponsePayload) || ResponsePayload.Length <= 1000)
                    return ResponsePayload;
                return ResponsePayload.Substring(0, 1000);
            }
        }

        public string ResponsePayloadCompressed
        {
            get
            {
                if (String.IsNullOrEmpty(ResponsePayload) || ResponsePayload.Length <= 1000)
                    return null;
                var bytes = Encoding.UTF8.GetBytes(ResponsePayload);
                using (var memStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress, true))
                    {
                        gzipStream.Write(bytes, 0, bytes.Length);
                    }
                    var compressed = memStream.ToArray();
                    return Convert.ToBase64String(compressed);
                }
            }
        }

        public virtual int TimeElapsed { get { return DateResponded.Subtract(DateRequested).Milliseconds; } }

        public void Dispose()
        {
            EndLog();
        }

        public Exception LogException(Exception exc)
        {
            Error = exc.Message;
            EndLog();
            return exc;
        }

        public void EndLog()
        {
            if (_logEnded) return;
            DateResponded = DateTime.UtcNow;
            _logEnded = true;
        }

        private bool _logEnded;
    }

    public enum JungoLoggerLog4NetDelimiter
    {
        Comma,
        Tab
    }

    [Serializable]
    public class JungoLoggerLog4NetConfig
    {
        public JungoLoggerLog4NetDelimiter Delimiter { get; set; }
        public List<JungoLoggerLog4NetColumn> Columns { get; set; }
        public string[] LogLevelsIncluded { get; set; }
    }

    [Serializable]
    public class JungoLoggerLog4NetColumn
    {
        public string Heading { get; set; }
        public string FieldName { get; set; }
    }
}

