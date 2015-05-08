using N2;
using N2.Engine;
using N2.Plugin;
using N2.Security;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
	/// <summary>
	/// Redirects user to login page on failed authorization
	/// </summary>
	[Service]	
	public class PermissionDeniedHandler : IAutoStart
	{
		readonly ISecurityEnforcer _securityEnforcer;
		readonly IWebContext _context;

		public PermissionDeniedHandler(ISecurityEnforcer securityEnforcer, IWebContext context)
		{
			_securityEnforcer = securityEnforcer;
			_context = context;
		}

		void securityEnforcer_AuthorizationFailed(object sender, CancellableItemEventArgs e)
		{
			var url = new Url("{ManagementUrl}/Login.aspx").ResolveTokens();
			url.AppendQuery("returnUrl", _context.Url.LocalUrl);
			_context.HttpContext.Response.Redirect(url);
		}

		#region IStartable Members

		public void Start()
		{
			_securityEnforcer.AuthorizationFailed += securityEnforcer_AuthorizationFailed;
		}

		public void Stop()
		{
			_securityEnforcer.AuthorizationFailed -= securityEnforcer_AuthorizationFailed;
		}

		#endregion
	}
}