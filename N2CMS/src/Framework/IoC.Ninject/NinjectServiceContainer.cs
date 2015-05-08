#define KEYS

using System;
using System.Collections.Generic;
using System.Linq;
using N2.Engine;
using N2.Plugin;
using Ninject;
using Ninject.Activation;
using Ninject.Activation.Strategies;
using Ninject.Components;
using Ninject.Parameters;

namespace N2.IoC.Ninject
{
#if DEBUG
    /// <summary>
    /// This class helps detect bad lifecycle definitions
    /// by S.Weber Dec-2013
    /// </summary>
    public class LogActivationStrategy : NinjectComponent, IActivationStrategy
    {
        private static readonly Dictionary<string, int> ActivationStats = new Dictionary<string, int>();

        public void Activate(IContext context, InstanceReference reference)
        {
            var count = 1;
            var clazz = reference.Instance.GetType().FullName;
            if (ActivationStats.ContainsKey(clazz))
                count = ActivationStats[clazz] + 1;

            ActivationStats[clazz] = count;
            if (count <= 1) return;

            var scope = context.Binding.BindingConfiguration.GetScope(context); // null = transient
            Logger.DebugFormat("Ninject Activate {0} {1} #{2} scope {3}", clazz, reference.Instance.GetHashCode(), count, scope);
        }

        public void Deactivate(IContext context, InstanceReference reference)
        {
            Logger.DebugFormat("Ninject Deactivate {0} {1}", reference.Instance.GetType().FullName, reference.Instance.GetHashCode());
        }
    }
#endif
    /// <summary>
    /// Wraps usage of the Ninject inversion of control container.
    /// </summary>
    public class NinjectServiceContainer : IServiceContainer
    {
        private static IKernel _kernel;
        private readonly Dictionary<Type, Type> _serviceTypeDefaultMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> _defaultServiceType = new Dictionary<Type, Type>(); 
        private readonly HashSet<string> _serviceClasses = new HashSet<string>();

#if KEYS
        private class KeyedService
        {
            public string Key { get; set; }
            public Type ServiceType { get; set; }
            public Type ClassType { get; set; }
        }

        private readonly Dictionary<string, KeyedService> _typeMap;
#endif
        private bool _componentsStarted;

        public static void SetKernel(IKernel kernel)
        {
            if (_kernel != null && kernel != null) // allow setting to null for unit tests
                throw new Exception("Kernel already set");

            _kernel = kernel;            
        }

        public NinjectServiceContainer() 
        {
            if (_kernel == null)
            {
                _kernel = new StandardKernel();
#if DEBUG
                _kernel.Components.Add<IActivationStrategy, LogActivationStrategy>();
#endif
            }
#if KEYS
            _typeMap = new Dictionary<string, KeyedService>();
#endif        
            _componentsStarted = false;
        }

        public void AddComponent(string key, Type serviceType, Type classType)
        {
            var serviceClass = classType.FullName;

            if (!_serviceClasses.Contains(serviceClass))
            {
                _kernel.Bind(serviceType).To(classType).InSingletonScope().Named(key);
                _serviceClasses.Add(serviceClass);
                _defaultServiceType[classType] = serviceType;
                _serviceTypeDefaultMap[serviceType] = serviceType;
                    _serviceTypeDefaultMap[classType] = classType;

#if KEYS
                _typeMap.Add(key, new KeyedService {Key = key, ClassType = classType, ServiceType = serviceType});
#endif

                if (_componentsStarted && typeof (IAutoStart).IsAssignableFrom(classType))
                {
                    ((IAutoStart) _kernel.Get(serviceType, key)).Start();
                }
            }
            else
            {
                _serviceTypeDefaultMap[serviceType] = _defaultServiceType[classType];
            }
        }

        public void AddComponentWithParameters(string key, Type serviceType, Type classType, IDictionary<string, string> properties1)
        {
            var binding = _kernel.Bind(serviceType).To(classType);
            properties1.Keys.ToList().ForEach(p => binding.WithParameter(new Parameter(p, properties1[p], false)));
#if KEYS
            _typeMap.Add(key, new KeyedService { Key = key, ClassType = classType, ServiceType = serviceType });
#endif        

            if (_componentsStarted && typeof(IAutoStart).IsAssignableFrom(classType))
            {
                ((IAutoStart)_kernel.Get(serviceType, key)).Start();
            }
        }


        public void AddComponentInstance(string key, Type serviceType, object instance)
        {
            _kernel.Bind(serviceType).ToMethod(ctx => instance).Named(key);
#if KEYS
            _typeMap.Add(key, new KeyedService {Key = key, ClassType = instance.GetType(), ServiceType = serviceType});
#endif        

            if (_componentsStarted && instance is IAutoStart)
            {
                ((IAutoStart)instance).Start();
            }
        }

        public void AddComponentLifeStyle(string key, Type serviceType, ComponentLifeStyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case ComponentLifeStyle.Singleton:
                    _kernel.Bind(serviceType).ToSelf().InSingletonScope().Named(key);
                    break;
                case ComponentLifeStyle.Transient:
                    _kernel.Bind(serviceType).ToSelf().InTransientScope().Named(key);
                    break;
            }
#if KEYS
            _typeMap.Add(key, new KeyedService{Key = key, ClassType = serviceType, ServiceType = serviceType});
#endif        

            if (_componentsStarted && typeof(IAutoStart).IsAssignableFrom(serviceType))
            {
                ((IAutoStart)_kernel.Get(serviceType, key)).Start();
            }
        }
        
        public object Resolve(Type type)
        {
            if (_serviceTypeDefaultMap.ContainsKey(type))
            {
                return _kernel.Get(_serviceTypeDefaultMap[type]);
            }

            return _kernel.Get(type);
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public T Resolve<T>(string key) where T : class
        {
            return _kernel.Get<T>(key);
        }

        public object Resolve(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _kernel.GetAll<T>();
        }

        public void Release(object instance)
        {
            _kernel.Release(instance);
        }

        public IEnumerable<ServiceInfo> Diagnose()
        {
#if KEYS
            return _typeMap.Values.Select(type => new ServiceInfo
                    {
                        ServiceType = type.ServiceType,
                        ImplementationType = type.ClassType,
                        Key = type.Key,
                        Resolve = () => Resolve(type.ServiceType),
                        ResolveAll = () => ResolveAll(type.ServiceType),
                        ServiceTypes = new[] { type.ServiceType }
                    });
#else
            throw new NotImplementedException();
#endif 
        }

        public static bool AutostartEnabled = true;

        public void StartComponents()
        {
            if (AutostartEnabled)
            {
#if KEYS
                foreach (var type in _typeMap.Values.Where(type => typeof (IAutoStart).IsAssignableFrom(type.ClassType)))
                {
                    ((IAutoStart) _kernel.Get(type.ServiceType, type.Key)).Start();
                }
#else
                throw new NotImplementedException();
#endif
                _componentsStarted = true;
            }
        }
    }
}
