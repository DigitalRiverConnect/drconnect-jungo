using System.Web;

namespace N2.Security
{
    // check if a request is considered secure, used to handle SSL termination at a BigIP
    public interface ISecureRequestCheck
    {
        bool IsSecure(HttpRequestBase request);
    }
}