using System.Collections.Concurrent;

namespace Jungo.Infrastructure.Cache
{
    public class InMemoryCache<T> : ICache<T>
    {
        private readonly ConcurrentDictionary<string, T> _cache;

        public InMemoryCache()
        {
            _cache = new ConcurrentDictionary<string, T>();
        }

        public void Add(string key, T value, int ttl = 30)
        {
            _cache.AddOrUpdate(key, value, (k, v) => v);
        }

        public bool TryGet(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void Remove(string key)
        {
            T oldValue;
            _cache.TryRemove(key, out oldValue);
        }

        public void Flush()
        {
            _cache.Clear();
        }
    }
}
