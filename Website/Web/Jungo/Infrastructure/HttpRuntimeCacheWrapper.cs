using System.Collections.Generic;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Cache;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    // wraps a dictionary that gets cached in HttpRuntime cache for current site 
    public class HttpRuntimeCacheWrapper<TValue> : ICache<TValue>, IFlushable
    {
        private static readonly object CacheLock = new object();
        private readonly Dictionary<string, TValue> _cacheDict;

        public HttpRuntimeCacheWrapper(string cacheKey, CacheWrapper cache)
        {
            lock (CacheLock)
            {
                _cacheDict = cache.Get<Dictionary<string, TValue>>(cacheKey);
                if (_cacheDict == null)
                {
                    _cacheDict = new Dictionary<string, TValue>();
                    cache.Add(cacheKey, _cacheDict);
                }
            } 
        }

        public void Add(string key, TValue value, int ttl = 30)
        {
            lock (CacheLock)
            {
                _cacheDict[key] = value;
            }
        }

        public bool TryGet(string key, out TValue value)
        {
            lock (CacheLock)
            {
                if (_cacheDict.ContainsKey(key))
                {
                    value = _cacheDict[key];
                    return true;
                }
                value = default(TValue);
                return false;
            }
        }

        public void Remove(string key)
        {
            lock (CacheLock)
            {
                if (_cacheDict.ContainsKey(key))
                {
                    _cacheDict.Remove(key);
                }
            }
        }

        public void Flush()
        {
            lock (CacheLock)
            {
                _cacheDict.Clear();
            }
        }
    }
}