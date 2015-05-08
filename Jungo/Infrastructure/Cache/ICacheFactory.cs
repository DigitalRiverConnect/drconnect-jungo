namespace Jungo.Infrastructure.Cache
{
    public interface ICacheFactory
    {
        ICache<T> GetCache<T>(string name);
    }
}
