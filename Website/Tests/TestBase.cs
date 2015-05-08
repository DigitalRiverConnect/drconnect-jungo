using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.SessionState;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Engine;
using N2.IoC.Ninject;
using Ninject;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests
{
	public class TestBase
	{
		public TestBase()
		{
            if (Singleton<IEngine>.Instance == null)
                Singleton<IEngine>.Instance = new ContentEngine(new NinjectServiceContainer());
			DependencyRegistrar = new DependencyRegistrar();
		}

		public DependencyRegistrar DependencyRegistrar { get; private set; }

		public FakeLogger FakeLogger
		{
			get { return DependencyRegistrar.FakeLogger; }
		}

        public static HttpContext FakeHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://dev-m.azu.digitalriver.com/microsoft.n2/msusa/en-US", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)
                                .Invoke(new object[] { sessionContainer });

            return httpContext;
        }

        protected void AssertIsCacheable<T>(T itemToCache)
        {

            Assert.DoesNotThrow(() =>
                {
                    var ms = new MemoryStream();
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, itemToCache);
                    ms.Flush();
                    Assert.True(ms.Length < 1048576);
                });
        }

	    protected static string[] SplitQueryParams(string query)
	    {
	        if (query.StartsWith("?"))
	            query = query.Remove(0, 1);
	        var qp = query.Split('&');
	        if (qp.Length == 1 && String.IsNullOrEmpty(qp[0]))
	            return new string[0];
	        return qp;
	    }
	}

	public class DependencyRegistrar
	{
		public DependencyRegistrar()
		{
            if (_kernel == null)
    			_kernel = new StandardKernel();
		}

		private static StandardKernel _kernel;

		private FakeLogger _fakeLogger;

		public FakeLogger FakeLogger
		{
			get
			{
				if (_fakeLogger == null)
					throw new Exception("you need to call WithFakeLogger() if you want access to a FakeLogger");
				return _fakeLogger;
			}
			private set { _fakeLogger = value; }
		}

		public DependencyRegistrar StandardDependencies()
		{
			//_kernel = new StandardKernel();
            _kernel.Unbind<IRequestLogger>();
            _kernel.Bind<IRequestLogger>();
            _kernel.Unbind<IDependencyResolver>();
            _kernel.Bind<IDependencyResolver>().ToMethod(
				ctx =>
				new NinjectDependencyResolver(ctx.Kernel))
                .InSingletonScope();
            _kernel.Unbind<IPageInfo>();
            _kernel.Bind<IPageInfo>().To<PageInfo>().InSingletonScope();
            DependencyResolver.Register(_kernel.Get<IDependencyResolver>());

			return this;
		}

		public DependencyRegistrar WithFakeLogger()
		{
			FakeLogger = new FakeLogger();
            _kernel.Unbind<IRequestLogger>();
            _kernel.Bind<IRequestLogger>().ToConstant(FakeLogger);
			return this;
		}
        
		public DependencyRegistrar With<T>(T obj)
		{
            _kernel.Unbind<T>();
			_kernel.Bind<T>().ToConstant(obj);
			return this;
		}

        public DependencyRegistrar WithMetadata<T>(T obj, string metadata)
        {
            _kernel.Unbind<T>();
            _kernel.Bind<T>().ToConstant(obj).WithMetadata(metadata, metadata);
            return this;
        }

		public DependencyRegistrar With<T>()
		{
            _kernel.Unbind<T>();
			_kernel.Bind<T>().ToSelf();
			return this;
		}

        public DependencyRegistrar WithSingleton<T>()
        {
            _kernel.Unbind<T>();
            _kernel.Bind<T>().ToSelf().InSingletonScope();
            return this;
        }

        public DependencyRegistrar WithSingleton<T, TC>() where TC : T
        {
            _kernel.Unbind<T>();
            _kernel.Bind<T>().To<TC>().InSingletonScope();
            return this;
        }
	}
}
