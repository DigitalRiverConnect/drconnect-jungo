using System;
using System.Net;
using System.Threading.Tasks;

namespace Jungo.Api
{
    public class ResponseInfo
    {
        public HttpStatusCode StatusCode { get; set; }
        public Uri Location { get; set; }
    }

    public interface IClient
    {
        string ApiKey { get; set; }
        string BearerToken { get; set; }
        string RefreshToken { get; set; }
        string SessionToken { get; set; }

        /// <summary>
        /// sets the ApiKey from configuration
        /// </summary>
        void SetApiKeyFromConfig();

        /// <summary>
        /// sets authorization header for subsequent calls
        /// also, after calling, BearerToken and RefreshToken are available
        /// <param name="forCartManagement">true if need access to be wired into gc magic for checkout</param>
        /// </summary>
        Task AuthenticateForLimitedPublicAsync(bool forCartManagement);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns>default(T) if not found</returns>
        Task<T> GetAsync<T>(string uri);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns>default(T) if not found</returns>
        Task<T> GetCacheableAsync<T>(string uri);
        Task<ResponseInfo> GetForInfoAsync(string uri);
        Task<ResponseInfo> PostForInfoAsync(string uri);
        Task<ResponseInfo> PostForInfoAsync(string uri, object payload);
        Task<T> PostAsync<T>(string uri);
        Task<T> PostAsync<T>(string uri, object payload);
        Task DeleteAsync(string uri);
    }
}
