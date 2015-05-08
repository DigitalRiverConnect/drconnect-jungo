using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using N2.Azure.Replication;
using DependencyResolver = Jungo.Infrastructure.DependencyResolver;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            RouteConfig.RegisterRoutes(routes);

#if DEBUG
            foreach (RouteBase re in RouteTable.Routes)
            {
                if (re is Route)
                {
                    var r = (Route)re;
                    N2.Engine.Logger.Info("> Route: " + r.Url + " -> " + r.RouteHandler.GetType().FullName);
                }
                else
                {
                    N2.Engine.Logger.Info("> Route: " + re.GetType().FullName);
                }
            }
#endif
        }

        protected void Application_Start()
        {
            // redirect N2 logging to DR library
            N2.Engine.Logger.WriterFactory = t => new N2Logger(
                                                      DependencyResolver.Current.Get
                                                          <ITraceLogger>());
            N2.Engine.Logger.Info(">>> MvcApplication Start");
            var engine = N2.Context.Initialize(false);

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // register Razor before WebForms
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ViewEngines.Engines.Add(new WebFormViewEngine());

            // App_Start not accessible in this library
            // WebApiConfig.Register(GlobalConfiguration.Configuration);
            // FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            // RouteConfig.RegisterRoutes(RouteTable.Routes);
            var bundleConfig = N2.Context.Current.Container.Resolve<BundleConfig>();
            if (bundleConfig != null)
                bundleConfig.RegisterBundles(BundleTable.Bundles);

            // initial sync of content
            var rep = engine.Resolve<ReplicationManager>();
            if (rep.IsSlave)
            {
                N2.Engine.Logger.Info("<<< MvcApplication Sync Data");
                while (rep.Syncronize(true) < 0)
                {
                    N2.Engine.Logger.Warn("Cannot sync due to locks. Will try again in a few seconds.");
                    Thread.Sleep(5000);
                }
                //var root = engine.Persister.Get(engine.Host.CurrentSite.RootItemID);
                //engine.Resolve<N2.Persistence.Search.IIndexer>().Update(root);
            }

            N2.Engine.Logger.Info("<<< MvcApplication Start");
        }
    }
}