using System.Data;
using N2.Engine;
using N2.Persistence.NH;

namespace N2.Persistence.Xml
{
	[Service(typeof (ISessionProvider), Configuration = "xml", Replaces = typeof (SessionProvider))]
	internal class XmlSessionProvider : ISessionProvider
	{
		public bool CacheEnabled
		{
			get { return true; }
		}

		public System.Data.IsolationLevel? Isolation
		{
			get { return IsolationLevel.Unspecified; }
		}

		public void Flush()
		{
		}

		public ITransaction BeginTransaction()
		{
			return null;
		}

		public ITransaction GetTransaction()
		{
			return null;
		}

		public void Dispose()
		{
		}

		public SessionContext OpenSession
		{
			get { return null; }
		}

		public NHibernate.ISessionFactory SessionFactory
		{
			get { return null; }
		}
	}
}
