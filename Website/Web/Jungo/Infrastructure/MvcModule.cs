//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  12/13/2012  HGodinez           Created
// 

using System;
using System.Linq;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Implementations.ShopperApiV1;
using Jungo.Infrastructure.Cache;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Logger;
using N2.Interfaces;
using Ninject;
using Ninject.Modules;
using ViewModelBuilders.Cart;
using ViewModelBuilders.Catalog;
using ViewModelBuilders.Layout;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    public class MvcModule : NinjectModule
    {
        protected static IKernel NinjectKernel;

        public override void Load()
        {
            NinjectKernel = Kernel; // remember for subsequent bindings

            Bind<IDependencyResolver>().ToMethod(
                ctx =>
                new NinjectDependencyResolver(ctx.Kernel)).
                InSingletonScope();

            // newer cache service
            Bind<ICacheFactory>().To<CacheFactory>().InSingletonScope();
            // newer config loader
            Bind<IConfigPathMapper>().To<HostingEnvironmentConfigPathMapper>().InSingletonScope();
            Bind<IConfigLoader>().To<JsonFileConfigLoader>().InSingletonScope();
            // newer crypto
            Bind<ICrypto>().To<Crypto>().InSingletonScope();

            Bind<IExternalWebLinkResolver>().To<ExternalWebLinkResolver>().InSingletonScope();

            Bind<IHttpModule>().To<JungoLoggingHttpModule>();
            Bind<IHttpModule>().To<SessionLogHttpModule>();
            Bind<IHttpModule>().To<ServerErrorHttpModule>();
            Bind<IHttpModule>().To<SessionHttpModule>();
            
            Bind<ISessionInfoResolver>().To<SessionInfoResolver>().InSingletonScope();

#if DEBUG_NOIIS
            Bind<ILogOnLinkGenerator>().To<N2LogOnLinkGenerator>().InSingletonScope();
#endif

            Bind<ITraceLogger>().To<TraceLogger>();
            Bind<IRequestLogger>().ToMethod(ctx => RequestLogger.Current);
            Bind<IClient>().ToMethod(ctx => ShopperApiClient.Current);
            Bind<ICatalogApi>().To<CatalogApi>();
            Bind<IOffersApi>().To<OffersApi>();
            Bind<ICartApi>().To<CartApi>();

            // Model Builders
            Bind<ICategoryViewModelBuilder>().To<CategoryViewModelBuilder>();
            Bind<IProductViewModelBuilder>().To<ProductViewModelBuilder>();
            Bind<IShoppingCartViewModelBuilder>().To<ShoppingCartViewModelBuilder>();
            Bind<IOffersViewModelBuilder>().To<OffersViewModelBuilder>();
            Bind<IOfferListViewModelBuilder>().To<OfferListViewModelBuilder>();
            Bind<IProductListViewModelBuilder>().To<ProductListViewModelBuilder>();
            Bind<ICategoryListViewModelBuilder>().To<CategoryListViewModelBuilder>();

            // Utilities
            Bind<IPageInfo>().To<PageInfo>().InSingletonScope();

            var logger = Kernel.Get<ITraceLogger>();

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (var loadedAssembly in loadedAssemblies)
            {
                logger.Debug("Loaded assembly: {0}", loadedAssembly.FullName);
            }
        }
    }
}