namespace Jungo.Infrastructure.Config
{
    public interface IConfigPathMapper
    {
        string Map(string virtualPath);
    }
}
