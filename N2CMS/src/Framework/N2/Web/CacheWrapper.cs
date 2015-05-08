using System;
using System.Web;
using System.Web.Caching;
using N2.Configuration;
using N2.Engine;
using N2.Persistence;
using N2.Web.UI;

namespace N2.Web
{
	public class CacheOptions
	{
		public CacheOptions()
		{
			AbsoluteExpiration = Cache.NoAbsoluteExpiration;
			SlidingExpiration = Cache.NoSlidingExpiration;
			Priority = CacheItemPriority.Normal;
			ContentDependency = true;
		}

		public bool ContentDependency { get; set; }

		public DateTime AbsoluteExpiration { get; set; }

		public TimeSpan SlidingExpiration { get; set; }

		public CacheItemPriority Priority { get; set; }

		public CacheItemRemovedCallback RemoveCallback { get; set; }
	}

	[Service]
	public class CacheWrapper
	{
	    readonly IPersister persister;
	    readonly string tablePrefix;
		private readonly string sqlCacheDependency;

		public CacheWrapper(IPersister persister, IWebContext context, DatabaseSection config)
		{
			this.persister = persister;
			this.tablePrefix = config.TablePrefix;
			this.sqlCacheDependency = config.SqlCacheDependency;
		}

		public virtual CacheDependency GetCacheDependency(CacheOptions options)
		{
			if (!options.ContentDependency)
				return null;
		
			if(string.IsNullOrEmpty(sqlCacheDependency))
				return new ContentCacheDependency(persister); // TODO ensure proper eviction with XML repo

			return new SqlCacheDependency(sqlCacheDependency, tablePrefix + "Item");
		}

		public virtual void Add(string cacheKey, object value, CacheOptions options = null)
		{
			if (options == null)
				options = new CacheOptions();

            HttpRuntime.Cache.Add(tablePrefix + cacheKey, value, GetCacheDependency(options), options.AbsoluteExpiration, options.SlidingExpiration, options.Priority, options.RemoveCallback);
		}

		public virtual void Remove(string cacheKey)
		{
            HttpRuntime.Cache.Remove(tablePrefix + cacheKey);
		}

		public virtual object Get(string cacheKey)
		{
            return HttpRuntime.Cache.Get(tablePrefix + cacheKey);
		}

		public virtual T Get<T>(string cacheKey) where T: class
		{
            return HttpRuntime.Cache.Get(tablePrefix + cacheKey) as T;
		}

		public virtual T GetOrCreate<T>(string cacheKey, Func<T> factory, CacheOptions options = null) where T : class
		{
			var value = Get<T>(cacheKey);
			if (value != null)
				return value;

			value = factory();
			Add(cacheKey, value, options);
			return value;
		}
	}
}
