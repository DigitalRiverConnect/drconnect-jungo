namespace Jungo.Infrastructure.Cache
{
    public interface ICache<T>
    {
        void Add(string key, T value, int ttl = 30);

        bool TryGet(string key, out T value);

        void Remove(string key);

        void Flush();
    }
}
