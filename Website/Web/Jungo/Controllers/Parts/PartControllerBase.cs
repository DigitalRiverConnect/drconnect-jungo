using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2;
using N2.Web.Mvc;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
	public class PartControllerBase<T> : ContentController<T> where T : ContentItem
	{
		protected readonly IRequestLogger Logger;
        protected PartControllerBase(IRequestLogger logger)
		{
			Logger = logger;
		}

		// for testing
		private string _sessionIdForTesting = string.Empty;
		public string SessionId
		{
			protected get { return string.IsNullOrEmpty(_sessionIdForTesting) ? WebSession.Current.SessionId : _sessionIdForTesting; }
			set { _sessionIdForTesting = value; }
		}

        // overridable for testing, to break dependency on WebSession, which depends on HttpContext which can't be mocked
        protected virtual TS WebSessionGet<TS>(string name) where TS : new()
        {
            return WebSession.Current.Get<TS>(name);
        }

		// overridable for testing, to break dependency on N2 context and request context
		protected virtual bool IsManaging()
		{
			return ((ContentController)this).IsManaging();
		}
	}
}
