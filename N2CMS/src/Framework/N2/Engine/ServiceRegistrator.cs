using System;
using System.Collections.Generic;
using System.Linq;

namespace N2.Engine
{
    /// <summary>
    /// Registers service in the N2 inversion of container upon start.
    /// </summary>
    public class ServiceRegistrator
    {
        private readonly ITypeFinder finder;
        private readonly IServiceContainer container;
        private Logger<ServiceRegistrator> Logger;

        public ServiceRegistrator(ITypeFinder finder, IServiceContainer container)
        {
            this.finder = finder;
            this.container = container;
        }

		public virtual IEnumerable<AttributeInfo<ServiceAttribute>> FindServices()
		{
			return finder.Find<ServiceAttribute>(typeof(object), false)
				.Where(t => !t.Type.IsAbstract)
				.Where(t => !t.Type.IsInterface)
				.Where(t => !t.Type.IsEnum)
				.Where(t => !t.Type.IsValueType)
				.Select(ai => new AttributeInfo<ServiceAttribute> { Attribute = ai.Attribute, DecoratedType = ai.Type });
		}

		public virtual void RegisterServices(IEnumerable<AttributeInfo<ServiceAttribute>> services)
		{
			var allServices = services.ToList();
			var replacementServices = allServices
				.Where(s => s.Attribute.Replaces != null)
				.Select(s => s.Attribute.Replaces).ToList();
			var addedServices = allServices
				.Where(s => !replacementServices.Contains(s.DecoratedType))
				.Where(s => s.Attribute.Replaces != null || s.Attribute.ServiceType == null || !replacementServices.Contains(s.Attribute.ServiceType))
                .OrderBy(s => (s.Attribute.ServiceType ?? s.DecoratedType).FullName + '>' + s.DecoratedType.FullName);

            // group registration by type
		    var serviceTypeNames = addedServices.Select(s => (s.Attribute.ServiceType ?? s.DecoratedType).FullName).Distinct();
            foreach (var name in serviceTypeNames)
            {
                var add = addedServices.Where(s => (s.Attribute.ServiceType ?? s.DecoratedType).FullName == name).ToArray();
                if (add.Count() == 1)
                    InternalRegisterService(add.First());
                else
                {
                    foreach (var info in add) { InternalRegisterService(info);}
                }
            }
		}

        private void InternalRegisterService(AttributeInfo<ServiceAttribute> info)
        {
            Type serviceType = info.Attribute.ServiceType ?? info.DecoratedType;
            string key = info.Attribute.Key ??
                         (info.Attribute.ServiceType ?? info.DecoratedType).FullName + "->" + info.DecoratedType.FullName;
            container.AddComponent(key, serviceType, info.DecoratedType);
            Logger.DebugFormat("InternalRegisterService {0} {1} {2}", key, serviceType.Name, info.DecoratedType.Name);
        }

        public virtual IEnumerable<AttributeInfo<ServiceAttribute>> FilterServices(IEnumerable<AttributeInfo<ServiceAttribute>> services, params string[] configurationKeys)
		{
			return services.Where(s => s.Attribute.Configuration == null || configurationKeys.Contains(s.Attribute.Configuration));
		}

		public virtual IEnumerable<AttributeInfo<ServiceAttribute>> FilterServices(IEnumerable<AttributeInfo<ServiceAttribute>> services, IEnumerable<Type> skipTypes)
		{
			return services.Where(s => !skipTypes.Contains(s.Attribute.ServiceType ?? s.DecoratedType));
		}
	}
}