using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Jungo.Infrastructure.Logger;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public class JungoLoggingHttpModule : BaseHttpModule
    {
        protected override void OnBeginRequest(HttpContext context, EventArgs e)
        {
            var requestLogger = new RequestLogger(inRequestScope: true);
            context.Items[RequestLogger.ItemKey] = requestLogger;
            RequestLogger.AddRequestLogger(requestLogger);
            CallContext.LogicalSetData(RequestLogger.RequestIdKey, requestLogger.RequestId);
            // the following code is designed to handle one and only one case: where we are handling the very first request since re-booting the app
            var zeroGuid = new Guid();
            var dummy = RequestLogger.GetRequestLogger(zeroGuid);
            if (dummy == null)
                RequestLogger.AddRequestLogger(zeroGuid, requestLogger);
            else
                RequestLogger.RemoveRequestLogger(zeroGuid);
        }

        protected override void OnMapRequest(HttpContext context, EventArgs e)
        {
            var requestLogger = context.Items[RequestLogger.ItemKey] as IRequestLogger;
            if (requestLogger == null || requestLogger.MvcLogRecord == null) return;

            requestLogger.MvcLogRecord.RequestType = String.Format("{0}.{1}",
                context.Request.RequestContext.RouteData.Values["controller"],
                context.Request.RequestContext.RouteData.Values["action"]);
        }

        protected override void OnEndRequest(HttpContext context, EventArgs e)
        {
            var requestLogger = context.Items[RequestLogger.ItemKey] as IRequestLogger;
            if (requestLogger == null) return;

            requestLogger.MvcLogRecord.DateResponded = DateTime.UtcNow;
            requestLogger.MvcLogRecord.HttpStatus = context.Response.StatusCode;
            requestLogger.MvcLogRecord.Error = GetExceptionMessage(context);
            requestLogger.WriteLog();
            RequestLogger.RemoveRequestLogger(requestLogger);
        }

        private static string GetExceptionMessage(HttpContext context)
        {
            if (context.AllErrors == null || context.AllErrors.Length <= 0) return null;
            var exc = context.AllErrors[0];
            var aggrExc = exc as AggregateException;
            if (aggrExc != null)
            {
                if (aggrExc.InnerException != null)
                    return aggrExc.InnerException.Message;
            }
            else
                return exc.Message;
            return null;
        }

        private readonly ITraceLogger _traceLogger;

        public JungoLoggingHttpModule(ITraceLogger traceLogger)
        {
            _traceLogger = traceLogger;
        }

        protected override ITraceLogger Logger
        {
            get { return _traceLogger; }
        }
    }
}