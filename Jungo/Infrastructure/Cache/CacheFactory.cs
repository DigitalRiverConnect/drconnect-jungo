using System.Collections.Concurrent;

namespace Jungo.Infrastructure.Cache
{
    public class CacheFactory : ICacheFactory
    {
        private static readonly ConcurrentDictionary<string, object> CacheDictionary = new ConcurrentDictionary<string, object>();

        public ICache<T> GetCache<T>(string name)
        {
            return CacheDictionary.GetOrAdd(name, key => new InMemoryCache<T>()) as ICache<T>;
        }
    }
}
