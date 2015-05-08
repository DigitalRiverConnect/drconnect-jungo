using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using N2.Engine;
using N2.Plugin;
using SimpleInjector;

namespace N2.IoC.SimpleInjector
{
    public class SimpleServiceContainer : IServiceContainer
	{
		private bool componentsStarted;
        public Container Ctr { get; set; }

		public SimpleServiceContainer()
		{
			Ctr = new Container();
            //Ctr.Options.AllowOverridingRegistrations = true;
		}

		public void AddComponent(string key, Type serviceType, Type classType)
		{
            if (serviceType == classType)
                Ctr.RegisterSingle(serviceType, serviceType);
            else
                Ctr.RegisterSingle(serviceType, classType);

            if (componentsStarted && typeof(IAutoStart).IsAssignableFrom(classType))
				(Ctr.GetInstance(serviceType) as IAutoStart).Start();
		}

		public void AddComponentInstance(string key, Type serviceType, object instance)
		{
            Ctr.RegisterSingle(serviceType, instance);

			if (componentsStarted && instance is IAutoStart)
				(instance as IAutoStart).Start();
		}

		public void AddComponentLifeStyle(string key, Type serviceType, ComponentLifeStyle lifeStyle)
		{
            switch (lifeStyle)
            {
                case ComponentLifeStyle.Singleton:
                    Ctr.RegisterSingle(serviceType);
                    break;
                case ComponentLifeStyle.Transient:
                    Ctr.Register(serviceType, serviceType,Lifestyle.Transient); // TODO check
                    break;
            }

            if (componentsStarted && typeof(IAutoStart).IsAssignableFrom(serviceType))
                (Resolve(serviceType) as IAutoStart).Start();
		}

		public void AddComponentWithParameters(string key, Type serviceType, Type classType, IDictionary<string, string> properties)
		{
			throw new NotImplementedException();
		}

		public T Resolve<T>() where T : class
		{
			return Ctr.GetInstance<T>();
		}

		public T Resolve<T>(string key) where T : class
		{
		    throw new NotImplementedException();
		}

		public object Resolve(Type type)
		{
            return Ctr.GetInstance(type);
		}

		public void Release(object instance)
		{
		}

		public IEnumerable<object> ResolveAll(Type serviceType)
		{
            var result = Ctr.GetAllInstances(serviceType);
		    if (result.Any())
		        return result;

            var list = new List<object>();
            var one = Ctr.GetInstance(serviceType);
		    if (one != null)
		        list.Add(one);

            return list;
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            var result = Ctr.GetAllInstances<T>();
            if (result.Any())
                return result;

            var list = new List<T>();
            var one = Ctr.GetInstance<T>();
            if (one != null)
                list.Add(one);

            return list;
        }

		public IEnumerable<ServiceInfo> Diagnose()
		{
			return Ctr.GetCurrentRegistrations()
				.Select(tr => new ServiceInfo { ServiceType = tr.ServiceType, 
                    ImplementationType = tr.Registration.ImplementationType,
                    Key = tr.Registration.ImplementationType.FullName,  // Keys are not supported by SimpleInjector
                    Resolve = () => Resolve(tr.ServiceType), 
                    ResolveAll = () => ResolveAll(tr.ServiceType), 
                    ServiceTypes = new [] { tr.ServiceType } });
		}



		public void StartComponents()
		{
		    foreach (var reg in Ctr.GetCurrentRegistrations().
                Where(reg => typeof(IAutoStart).IsAssignableFrom(reg.Registration.ImplementationType ?? reg.ServiceType)))
		    {
		        ((IAutoStart) reg.GetInstance()).Start();
		    }

		    componentsStarted = true;
		}
	}

}
