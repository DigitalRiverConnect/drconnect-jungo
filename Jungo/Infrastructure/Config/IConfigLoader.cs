namespace Jungo.Infrastructure.Config
{
    public interface IConfigLoader
    {
        T Get<T>() where T : class;
    }
}
