using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Attributes;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using N2.IoC.Ninject;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Ninject.Web.Mvc.FilterBindingSyntax;
using DependencyResolver = Jungo.Infrastructure.DependencyResolver;
using IDependencyResolver = Jungo.Infrastructure.IDependencyResolver;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            //var kernel = new StandardKernel(new NinjectSettings() { UseReflectionBasedInjection = true }); 
            // medium trust - see https://github.com/ninject/ninject.web.mvc/issues/15
            var kernel = new StandardKernel();
//            var kernel = new StandardKernel(new NinjectSettings { LoadExtensions = false });
//            kernel.Load(new Ninject.Web.Mvc.MvcModule());
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);

            // this will ensure that n2 uses the same kernel
            NinjectServiceContainer.SetKernel(kernel);

            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            var modules = new List<NinjectModule>
                              {
                                  new MvcModule()
                              };
            kernel.Load(modules);

            kernel.BindFilter<ErrorLoggingAttribute>(FilterScope.Global, 1);
            //kernel.BindFilter<ValidateN2ShopperProfileAttribute>(FilterScope.Global, 4);

            DependencyResolver.Register(
                kernel.Get<IDependencyResolver>());
        }        
    }
}
