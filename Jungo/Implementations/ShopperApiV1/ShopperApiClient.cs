using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using Jungo.Api;
using Jungo.Implementations.ShopperApiV1.Config;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Cache;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Logger;
using Jungo.Models.Oauth20;
using Jungo.Models.ShopperApi.Common;
using Newtonsoft.Json;

namespace Jungo.Implementations.ShopperApiV1
{
    public class ShopperApiClient : IClient
    {
        public ShopperApiClient(ICacheFactory cacheFactory, bool refreshingCache = false)
        {
            _cache = cacheFactory.GetCache<string>("ShortTerm");
            _longTermCache = cacheFactory.GetCache<string>("LongTerm");
            _httpClient = new HttpClient(new HttpClientHandler {AllowAutoRedirect = false});
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _refreshingCache = refreshingCache;
        }

        public static void AddClient(Guid requestId, IClient client)
        {
            _clients.TryAdd(requestId, client);
        }

        public static IClient GetClient(Guid requestId)
        {
            IClient client;
            return _clients.TryGetValue(requestId, out client) ? client : null;
        }

        public static void RemoveClient(Guid requestId)
        {
            IClient client;
            _clients.TryRemove(requestId, out client);
        }

        public static IClient Current
        {
            get
            {
                var requestId = CallContext.LogicalGetData(RequestLogger.RequestIdKey);
                var client = requestId != null ? GetClient((Guid)requestId) : GetClient(new Guid());
                if (client != null)
                    return client;
                if (HttpContext.Current != null && HttpContext.Current.Items[Constants.ShopperApiClientHttpContextItemKey] != null)
                    return HttpContext.Current.Items[Constants.ShopperApiClientHttpContextItemKey] as IClient;
                return new ShopperApiClient(DependencyResolver.Current.Get<ICacheFactory>());
            }
        }

        private readonly bool _refreshingCache;

        private readonly HttpClient _httpClient;
        private static readonly MediaTypeFormatter[] Formatters = {new JsonMediaTypeFormatter()};

        private static readonly ConcurrentDictionary<string, string> InProcessRequests =
            new ConcurrentDictionary<string, string>();

        private readonly ICache<string> _cache;
        private readonly ICache<string> _longTermCache;

        private static ConcurrentDictionary<Guid, IClient> _clients = new ConcurrentDictionary<Guid, IClient>();

        #region IShopperApiClient Members

        private string _bearerToken;

        public string BearerToken
        {
            get { return _bearerToken; }
            set
            {
                _bearerToken = value;
                if (!string.IsNullOrEmpty(_bearerToken))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _bearerToken);
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
        }

        public string ApiKey { get; set; }
        public string RefreshToken { get; set; }
        public string SessionToken { get; set; }

        public void SetApiKeyFromConfig()
        {
            ApiKey = ConfigLoader.Get<ShopperApiKeyConfig>().ApiKey;
        }

        public async Task AuthenticateForLimitedPublicAsync(bool forCartManagement)
        {
            if (forCartManagement && string.IsNullOrEmpty(SessionToken))
            {
                var sessionTokenUri = Billboard.GetSessionTokenUri();
                var sessionTokenResponse = await GetAsync<SessionToken>(sessionTokenUri).ConfigureAwait(false);
                if (sessionTokenResponse != null)
                    SessionToken = sessionTokenResponse.session_token;
            }
            var billboard = await Billboard.GetResourceAccessAsync(this).ConfigureAwait(false);
            var tokenUri = FormLimitedPublicTokenUri(billboard);
            var log = BeginLog("POST", tokenUri);
            var response = await PostLimitedPublicTokenRequestAsync(tokenUri).ConfigureAwait(false);
            LogResponse(log, response);
            await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            EndLog(log, String.Empty);
            await SetTokensAsync(response).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var json = await GetStringAsync(uri).ConfigureAwait(false);
            return string.IsNullOrEmpty(json) ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T> GetCacheableAsync<T>(string uri)
        {
            var json = await GetCacheableStringAsync(uri).ConfigureAwait(false);
            return string.IsNullOrEmpty(json) ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<ResponseInfo> GetForInfoAsync(string uri)
        {
            var log = BeginLog("GET", uri);
            var response = await RetryIfTokenExpired(() => _httpClient.GetAsync(uri)).ConfigureAwait(false);
            LogResponse(log, response);
            if (!IsRedirect(response))
                await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            EndLog(log, String.Empty);
            return new ResponseInfo { StatusCode = response.StatusCode, Location = response.Headers.Location };
        }

        public async Task<ResponseInfo> PostForInfoAsync(string uri)
        {
            var log = BeginLog("POST", uri);
            var response = await RetryIfTokenExpired(() => _httpClient.PostAsync(uri, null)).ConfigureAwait(false);
            LogResponse(log, response);
            if (!IsRedirect(response))
                await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            EndLog(log, String.Empty);
            return new ResponseInfo { StatusCode = response.StatusCode, Location = response.Headers.Location };
        }

        public async Task<ResponseInfo> PostForInfoAsync(string uri, object payload)
        {
            var log = BeginLog("POST", uri, payload);
            var response = await RetryIfTokenExpired(
                () => _httpClient.PostAsync(uri, payload, Formatters[0])).ConfigureAwait(false);
            LogResponse(log, response);
            if (!IsRedirect(response))
                await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            EndLog(log, String.Empty);
            return new ResponseInfo { StatusCode = response.StatusCode, Location = response.Headers.Location };
        }

        public async Task<T> PostAsync<T>(string uri)
        {
            var log = BeginLog("POST", uri);
            var response = await RetryIfTokenExpired(() => _httpClient.PostAsync(uri, null)).ConfigureAwait(false);
            LogResponse(log, response);
            await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            var responsePayload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            EndLog(log, responsePayload);
            return JsonConvert.DeserializeObject<T>(responsePayload);
        }

        public async Task<T> PostAsync<T>(string uri, object payload)
        {
            var log = BeginLog("POST", uri, payload);
            var response = await RetryIfTokenExpired(
                () => _httpClient.PostAsync(uri, payload, Formatters[0])).ConfigureAwait(false);
            LogResponse(log, response);
            await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            var responsePayload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            EndLog(log, responsePayload);
            return JsonConvert.DeserializeObject<T>(responsePayload);
        }

        public async Task DeleteAsync(string uri)
        {
            var log = BeginLog("DELETE", uri);
            var response = await RetryIfTokenExpired(() => _httpClient.DeleteAsync(uri)).ConfigureAwait(false);
            LogResponse(log, response);
            await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            EndLog(log, null);
        }

        #endregion

        private async Task ThrowIfFailAsync(HttpResponseMessage resp, LogRecord log)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var content = string.Empty;
                if (resp.Content != null)
                    content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                ThrowFailed(resp, log, content);
            }
        }

        private void ThrowIfFail(HttpResponseMessage resp, LogRecord log, string content)
        {
            if (!resp.IsSuccessStatusCode)
                ThrowFailed(resp, log, content);
        }

        private void ThrowFailed(HttpResponseMessage resp, LogRecord log, string content)
        {
            LogException(log, content);
            throw new ShopperApiException
            {
                Uri = resp.RequestMessage.RequestUri.ToString(),
                HttpStatusCode = resp.StatusCode,
                ShopperApiError = ExtractError(content)
            };
        }

        private static Error ExtractError(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            // sometime content is a json string, sometimes it is not;
            //  so, we have to do some checking on the content
            if (!content.StartsWith("{") && !content.StartsWith("["))
                return new Error { Description = content };
            try
            {
                return JsonConvert.DeserializeObject<ErrorsResponse>(content).Errors.Error;
            }
            catch (Exception)
            {
                // the content string was not parsable, so ...
                return new Error { Description = content };
            }
        }

        private static bool IsRedirect(HttpResponseMessage resp)
        {
            return resp.StatusCode == HttpStatusCode.Moved ||
                   resp.StatusCode == HttpStatusCode.Redirect;
        }

        private async Task<string> GetCacheableStringAsync(string uri)
        {
            var log = BeginLog("GET", uri);
            string result;
            // Try to get from the both caches. If both are empty, retrieve synchronously and return the value
            if (_cache.TryGet(uri, out result))
            {
                EndLog(log, String.Empty, DataFlow.FromShortTermCache);
                return result;
            }

            if (!_longTermCache.TryGet(uri, out result))
            {
                var json = await GetStringAsync(uri).ConfigureAwait(false);
                return CacheResult(uri, json); // Nulls should also be cached, otherwise, we can get overwhelmed when someone repeatedly asks for something that returns null.
            }

            // If another thread is already retrieving the value, do not attempt to retrieve it yet again.
            if (!InProcessRequests.TryAdd(uri, uri))
            {
                EndLog(log, String.Empty, DataFlow.FromLongTermCache);
                return result;
            }

            // If another thread had already retrieved the value during the RetrievalInProcessCheck above, do not attempt to retrieve it again.
            string newResult;
            if (_cache.TryGet(uri, out newResult))
            {
                EndLog(log, String.Empty, DataFlow.FromShortTermCache);
                return newResult;
            }

            // at this point, we know it's in the long term cache, but not the short term cache, so
            // we get the new values asynchronously and cache it. We also then return the long term cache value
            // in deserialized form.
#pragma warning disable 4014
            Task.Factory.StartNew(() =>
#pragma warning restore 4014
            {
// ReSharper disable once UseObjectOrCollectionInitializer
                var bkgndRequestLogger = new RequestLogger(inRequestScope: false);
                var reqLogger = RequestLogger.Current;
                bkgndRequestLogger.SessionId = reqLogger.SessionId;
                bkgndRequestLogger.RequestId = reqLogger.RequestId;
                try
                {

                    var bkgndClient = new ShopperApiClient(DependencyResolver.Current.Get<ICacheFactory>(), true)
                    {
                        ApiKey = ApiKey,
                        BearerToken = BearerToken,
                        RefreshToken = RefreshToken
                    };
                    var json = bkgndClient.GetStringAsync(uri).Result;
                    CacheResult(uri, json); // Nulls should also be cached, otherwise, we can get overwhelmed when someone repeatedly asks for something that returns null.
                }
                finally
                {
                    string oldUri;
                    InProcessRequests.TryRemove(uri, out oldUri);
                    bkgndRequestLogger.WriteLog();
                }
            });

            EndLog(log, String.Empty, DataFlow.FromLongTermCache);
            return result;
        }

        private async Task<string> GetStringAsync(string uri, bool bNoRetry = false)
        {
            var log = BeginLog("GET", uri);
            HttpResponseMessage response;
            if (bNoRetry)
                response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            else
                response = await RetryIfTokenExpired(() => _httpClient.GetAsync(uri)).ConfigureAwait(false);
            LogResponse(log, response);
            string payload = null;
            if (response.Content != null)
                payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // for an ordinary "resource not found" such as a wrong prod id, we return null string
                // but if uri is totally nutso cuckoo, throw not found
                if (string.IsNullOrEmpty(payload) || ExtractError(payload).Relation.EndsWith("shoppers/errors"))
                    ThrowIfFail(response, log, payload);
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // for an ordinary "conflict" we will throw and exception
                // but if the error "code" is in our configured list of codes to treat as 404's, return null
                var config = ConfigLoader.Get<Psuedo404ErrorCodesConfig>();
                if (!config.Codes.Contains(ExtractError(payload).Code))
                    ThrowIfFail(response, log, payload);
            }
            else
                await ThrowIfFailAsync(response, log).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Conflict)
                payload = null;
            EndLog(log, payload, _refreshingCache ? DataFlow.CacheRefresh : DataFlow.FromShopperApi);
            return payload;
        }

        private string CacheResult(string cacheKey, string value)
        {
            _cache.Add(cacheKey, value);
            _longTermCache.Add(cacheKey, value, 2880);
            return value;
        }

        private async Task<HttpResponseMessage> RetryIfTokenExpired(Func<Task<HttpResponseMessage>> req)
        {
            // BEWARE: Don't call any higher-level Get or Post here which might themselves do a retry.
            var response = await req().ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Unauthorized ||
                _httpClient.DefaultRequestHeaders.Authorization == null ||
                string.IsNullOrEmpty(RefreshToken))
                return response;

            // perhaps bearer token is expired
            var billboard = await Billboard.GetResourceAccessAsync(this).ConfigureAwait(false);

            if (SessionToken == null)
            {
                // since no session token is involved, try to use refresh token to get new bearer token
                var refreshUri = Billboard.ResolveTemplate(billboard.Token.Uri, Billboard.Templates.RefreshTokenQuery,
                    new {client_id = ApiKey, refresh_token = RefreshToken});
                _httpClient.DefaultRequestHeaders.Authorization = null;
                response = await _httpClient.PostAsync(refreshUri, null).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Unauthorized)
                    return response;
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // refresh token might be expired; start over
                    response = await PostLimitedPublicTokenRequestAsync(FormLimitedPublicTokenUri(billboard)).ConfigureAwait(false);
                    if (response.StatusCode != HttpStatusCode.OK) return response;
                }
            }
            else
            {
                // todo: when we implement shopper authentication, if we have a shopper, this needs to be the shopper token request, not limited public
                // if used a session token to get bearer token, unfortunately refresh token is doo doo
                // start over with existing session token if have one
                if (!string.IsNullOrEmpty(SessionToken))
                    response = await PostLimitedPublicTokenRequestAsync(FormLimitedPublicTokenUri(billboard)).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // perhaps session token is expired; need to get a new session token
                    // our thoughts and prayers go out to the poor shopper who is now going to lose their cart contents
                    var sessionTokenUri = Billboard.GetSessionTokenUri();
                    var json = await GetStringAsync(sessionTokenUri, true/*no retry!*/).ConfigureAwait(false); 
                    if (!string.IsNullOrEmpty(json))
                    {
                        var sessionTokenResponse = JsonConvert.DeserializeObject<SessionToken>(json);
                        if (sessionTokenResponse != null)
                            SessionToken = sessionTokenResponse.session_token;
                    }
                    response = await PostLimitedPublicTokenRequestAsync(FormLimitedPublicTokenUri(billboard)).ConfigureAwait(false);
                    if (response.StatusCode != HttpStatusCode.OK)
                        return response;
                }
            }

            // whew!!! we got reauthorized
            await SetTokensAsync(response).ConfigureAwait(false);

            // replay request
            return await req().ConfigureAwait(false);
        }

        private string FormLimitedPublicTokenUri(ResourceAccessBillboard billboard)
        {
            var queryParms = string.IsNullOrEmpty(SessionToken)
                ? new {client_id = ApiKey}
                : (object) new {client_id = ApiKey, dr_session_token = SessionToken};
            return Billboard.ResolveTemplate(billboard.Token.Uri, Billboard.Templates.LimitedPublicTokenQuery,
                queryParms);
        }

        private async Task<HttpResponseMessage> PostLimitedPublicTokenRequestAsync(string tokenUri)
        {
            return await _httpClient.PostAsync(tokenUri, null).ConfigureAwait(false);
        }

        private async Task SetTokensAsync(HttpResponseMessage response)
        {
            var token = await response.Content.ReadAsAsync<Token>(Formatters).ConfigureAwait(false);
            BearerToken = token.access_token;
            RefreshToken = token.refresh_token;
        }

        #region Logging helpers

        private static ShopperApiLogRecord BeginLog(string httpMethod, string uri, object payload = null,
            [CallerMemberName] string memberName = "")
        {
            var log = new ShopperApiLogRecord
            {
                Host = Environment.MachineName,
                RequestType = String.Format("ShopperApiClient.{0}", memberName),
                HttpMethod = httpMethod,
                Uri = uri,
                RequestPayload = payload == null ? null : JsonConvert.SerializeObject(payload)
            };
            return log;
        }

        private void EndLog(ShopperApiLogRecord log, string payload, DataFlow dataFlow = DataFlow.FromShopperApi)
        {
            if (log == null) return;
            log.ResponsePayload = payload;
            log.DateResponded = DateTime.UtcNow;
            log.DataFlow = dataFlow;
            RequestLogger.Current.AddLogRecord(log);
        }

        private static void LogResponse(ShopperApiLogRecord log, HttpResponseMessage response)
        {
            if (log == null) return;
            log.HttpStatus = (int) response.StatusCode;
            if (response.Headers.Contains("x-dr-requestid"))
                log.DrRequestId = response.Headers.GetValues("x-dr-requestid").FirstOrDefault();
        }

        private void LogException(LogRecord log, string content)
        {
            if (log == null) return;
            log.Error = content;
            log.DateResponded = DateTime.UtcNow;
            RequestLogger.Current.AddLogRecord(log);
        }

        #endregion
    }
}
