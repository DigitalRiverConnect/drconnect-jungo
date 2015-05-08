using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Engine;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services
{
	/// <summary>
	/// N2Logger that maps to ILogger
	/// </summary>
	public class N2Logger : LogWriterBase
	{
		private readonly ITraceLogger _log;

        public N2Logger(ITraceLogger log) 
		{
			_log = log;
		}

		public override void Error(string message)
		{
			_log.Error("{0}", message);
		}

		public override void Error(string format, object[] args)		
		{
			_log.Error(format, args);
		}

		public override void Warning(string message)
		{
			_log.Warn("{0}", message);
		}

		public override void Warning(string format, object[] args)
		{
			_log.Warn(format, args);
		}

		public override void Information(string message)
		{
			_log.Info("{0}", message);
		}

		public override void Information(string format, object[] args)
		{
			_log.Info(format, args);
		}

		public override void Debug(string message)
		{
			_log.Debug("{0}", message);
		}

		public override void Debug(string format, object[] args)
		{
			_log.Debug(format, args);
		}
	}
}
