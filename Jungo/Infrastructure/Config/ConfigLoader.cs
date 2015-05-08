namespace Jungo.Infrastructure.Config
{
    public static class ConfigLoader
    {
        public static T Get<T>() where T : class
        {
            var loader = GetLoader();
            return loader.Get<T>();
        }

        private static IConfigLoader _configLoader;

        private static IConfigLoader GetLoader()
        {
            return _configLoader ?? (_configLoader = DependencyResolver.Current.Get<IConfigLoader>());
        }
    }
}
