//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  12/13/2012  HGodinez           Created
// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Logger;
using N2.Engine;
using Newtonsoft.Json;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session
{
    public class WebSession : IDisposable
    {
        public const string ItemKey = "_webSession_";
        public const string SessionTokenSlot = "SessionToken";
        public const string SecurityTokenSlot = "SecurityToken";

        public const string NameOnProfileSlot = "NameOnProfile";
            // TODO: this is "__NameOnProfile__" in n2cms project, WITH the underscores

        public const string SearchResultSlot = "Products";
        public const string SearchHitsSlot = "SearchHits";
        public const string CategoryIdSlot = "CategoryId";
        public const string CurrentProductSlot = "Product";
        public const string HasCartSlot = "HasCart";
        public const string ShoppingCartSlot = "Cart";
        public const string ShoppingCartCount = "CartCount";
        private const string SiteInfoSlot = "SiteInfo";
        public const string McIdSlot = "McId";
        public const string IcIdSlot = "IcId";

        public const string CheckoutBreadcrumbsSteps = "CheckoutBreadcrumbsSteps";
        public const string CheckoutThreeSteps = "CheckoutThreeSteps";
        public const string CheckoutFourSteps = "CheckoutFourSteps";
        public const string NewShippingAddressFirstName = "NewShippingAddressFirstName";
        public const string NewShippingAddressLastName = "NewShippingAddressLastName";

        public const string MarketPlaceParameter = "mktp";

//        private const string Slot = ".WS";
        private const string SessionTokenCookieName = ".ASPXTOKEN";
        private const string SecurityTokenCookieName = ".ASPXSTOKEN";
        private const string McIdCookieName = "WT.mc_id";
        private const string IcIdCookieName = "Icid";
        private const string AfterAddToCartOptionSessionName = "AfterAddToCartOption";
        private const string PersistentPropertiesCookieName = "PersistentProperties";

        private static ConcurrentDictionary<Guid, WebSession> _webSessions = new ConcurrentDictionary<Guid, WebSession>();
        private Dictionary<string, object> _sessionData;

        protected WebSession()
        {
            _sessionData = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        }

        private static void AddWebSession(Guid requestId, WebSession webSession)
        {
            _webSessions.TryAdd(requestId, webSession);
        }

        private static WebSession GetWebSession(Guid requestId)
        {
            WebSession webSession;
            return _webSessions.TryGetValue(requestId, out webSession) ? webSession : null;
        }

        private static void RemoveWebSession(Guid requestId)
        {
            WebSession webSession;
            _webSessions.TryRemove(requestId, out webSession);
        }

        public static WebSession Current
        {
            get
            {
                var requestId = CallContext.LogicalGetData(RequestLogger.RequestIdKey);
                var webSession = requestId != null ? GetWebSession((Guid)requestId) : GetWebSession(new Guid());
                if (webSession != null)
                    return webSession;
                if (HttpContext.Current != null && HttpContext.Current.Items[ItemKey] != null)
                    return HttpContext.Current.Items[ItemKey] as WebSession;
                return null;
            }
            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Items[ItemKey] = value;
                var requestId = CallContext.LogicalGetData(RequestLogger.RequestIdKey);
                AddWebSession(requestId != null ? (Guid)requestId : new Guid(), value);
                // the following code is designed to handle one and only one case: where we are handling the very first request since re-booting the app
                var zeroGuid = new Guid();
                var dummy = GetWebSession(zeroGuid);
                if (dummy == null)
                    AddWebSession(zeroGuid, value);
                else
                    RemoveWebSession(zeroGuid);
            }
        }

        public static bool IsInitialized
        {
            get
            {
                var requestId = CallContext.LogicalGetData(RequestLogger.RequestIdKey);
                var webSession = requestId != null ? GetWebSession((Guid)requestId) : GetWebSession(new Guid());
                return webSession != null;
            }
        }

        public static void BeginSession(HttpContext context, ICrypto cryptographicService)
        {
            SessionToken sessionToken = null;
            bool isCookieDecrypted = false;
            bool isSessionTokenParsed = false;
//            bool isNewSession = false;

            // we must set this context value before trying to get site info because site info may be affected by it
            var mktp = (string) context.Items[MarketPlaceParameter];
            if (String.IsNullOrEmpty(mktp))
            {
                var mktpCookie = context.Request.Cookies.Get(MarketPlaceParameter);
                if (mktpCookie != null)
                    context.Items[MarketPlaceParameter] = mktpCookie.Value;
            }

            SiteInfo siteInfo;

            var sessionCookie = context.Request.Cookies.Get(SessionTokenCookieName);
            if (sessionCookie != null)
            {
                string decryptedSessionCookieValue;
                isCookieDecrypted = cryptographicService.TryDecrypt(sessionCookie.Value, out decryptedSessionCookieValue);
                isSessionTokenParsed = SessionToken.TryParse(decryptedSessionCookieValue, out sessionToken);
            }

            var destroySecurityToken = false;
            if (TryGetSiteInfo(context, out siteInfo))
            {
                if (sessionCookie != null && isCookieDecrypted && isSessionTokenParsed)
                {
                    if (!String.Equals(sessionToken.SiteId, siteInfo.SiteId) ||
                        !String.Equals(sessionToken.CultureCode, siteInfo.Locale) ||
                        !String.Equals(sessionToken.CountryCode, siteInfo.Country))
                    {

                        var preSessionId = PreSessionId();
                        sessionToken = new SessionToken(siteInfo.SiteId, siteInfo.Locale, siteInfo.Country,
                            siteInfo.Currency, context.GetExternalId(), preSessionId);

                        destroySecurityToken = true;
//                        isNewSession = true;
                    }
                }
                else
                {
                    var preSessionId = !String.IsNullOrEmpty(siteInfo.SiteId)
                        ? PreSessionId()
                        : null;

                    if (preSessionId == null)
                    {
                        sessionToken = new SessionToken(siteInfo.SiteId, null, null, null, context.GetExternalId(), null);
//                        isNewSession = true;
                    }
                    else
                    {
                        sessionToken = new SessionToken(siteInfo.SiteId, siteInfo.Locale, siteInfo.Country,
                            siteInfo.Currency, context.GetExternalId(), preSessionId);
                    }

                    destroySecurityToken = true;
                }
            }

            WebSession session = null;
            if (sessionToken != null)
            {
                session = new WebSession();
                session.Set(SessionTokenSlot, sessionToken);
//                session.IsNewSession = isNewSession;
                if (siteInfo != null) session.Set(SiteInfoSlot, siteInfo);
            }


            RequestParmToCookie(context, session, McIdCookieName, McIdSlot);
            RequestParmToCookie(context, session, IcIdCookieName, IcIdSlot);

            if (destroySecurityToken)
            {
                if (context.Request.Cookies[SecurityTokenCookieName] != null)
                {
                    context.Response.Cookies.Remove(SecurityTokenCookieName);
                }
            }
            else
            {
                var securityTokenCookie = context.Request.Cookies.Get(SecurityTokenCookieName);
                string decryptedSecurityTokenCookieValue;
                if (session != null && securityTokenCookie != null &&
                    cryptographicService.TryDecrypt(securityTokenCookie.Value, out decryptedSecurityTokenCookieValue))
                {
                    session.Set(SecurityTokenSlot, JsonConvert.DeserializeObject<SecurityToken>(decryptedSecurityTokenCookieValue));
                }
            }

            Current = session;
            if (session == null)
                return;

            var persistentPropertiesCookie = context.Request.Cookies.Get(PersistentPropertiesCookieName);
            if (persistentPropertiesCookie == null)
                return;

            string decryptedPersistentPropertiesCookieValue;
            if (cryptographicService.TryDecrypt(persistentPropertiesCookie.Value,
                out decryptedPersistentPropertiesCookieValue))
                session.PersistentProperties =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedPersistentPropertiesCookieValue);
        }

        public void ResetSessionId(string sessionId)
        {
            SessionToken sessionToken;
            if (!TryGet(SessionTokenSlot, out sessionToken))
                return;

            sessionToken.ResetSessionId(sessionId);
            Set(SessionTokenSlot, sessionToken);
        }

        public static void EndSession(HttpContext context, ICrypto cryptographicService)
        {

            if (Current != null)
            {
                var webSession = Current;

                SessionToken sessionToken;
                if (webSession.TryGet(SessionTokenSlot, out sessionToken))
                {
                    var sessionCookie = new HttpCookie(SessionTokenCookieName,
                        cryptographicService.Encrypt(sessionToken.ToString())) {HttpOnly = true};
                    try
                    {
                        context.Response.Cookies.Set(sessionCookie);
                    }
                    catch (Exception exception)
                    {
                        Logger.WarnFormat("Unable to set session cookie. {0}", exception.ToString());
                    }
                }

                SecurityToken securityToken;
                if (webSession.TryGet(SecurityTokenSlot, out securityToken))
                {
                    var securityTokenCookie = new HttpCookie(SecurityTokenCookieName,
                        cryptographicService.Encrypt(JsonConvert.SerializeObject(securityToken)))
                    {
                        Expires = securityToken.TokenExpiration,
                        HttpOnly = true
                    };
                    try
                    {
                        context.Response.Cookies.Set(securityTokenCookie);
                    }
                    catch (Exception exception)
                    {
                        Logger.WarnFormat("Unable to set security token cookie. {0}", exception.ToString());
                    }
                }
                else if (context.Request.Cookies[SecurityTokenCookieName] != null)
                {
                    var existingCookie = context.Request.Cookies[SecurityTokenCookieName];
                    if (existingCookie != null)
                    {
                        existingCookie.Expires = DateTime.Now.AddDays(-1);
                        existingCookie.HttpOnly = true;
                        context.Response.Cookies.Set(existingCookie);
                    }
                }

                if (webSession.PersistentProperties != null && webSession.PersistentProperties.Count > 0)
                {
                    // we need to chunk these serialized properties to avoid overrunning coolie size limits!!
                    var persistentPropertiesCookie = new HttpCookie(PersistentPropertiesCookieName,
                        cryptographicService.Encrypt(JsonConvert.SerializeObject(webSession.PersistentProperties)));
                    try
                    {
                        context.Response.Cookies.Set(persistentPropertiesCookie);
                    }
                    catch (Exception exception)
                    {
                        Logger.WarnFormat("Unable to set Persistent Properites cookie. {0}",
                            exception.ToString());
                    }
                }
                var mktp = (string) context.Items[MarketPlaceParameter];
                if (!String.IsNullOrEmpty(mktp))
                    context.Response.Cookies.Set(new HttpCookie(MarketPlaceParameter, mktp));

                var requestId = CallContext.LogicalGetData(RequestLogger.RequestIdKey);
                if (requestId != null)
                    RemoveWebSession((Guid)requestId);
            }
        }

        public virtual T Get<T>(string name)
        {
            object value;
            if (_sessionData.TryGetValue(name, out value))
                return (T) value;
            return default(T);
        }

        public virtual bool TryGet<T>(string name, out T value)
        {
            var result = false;

            try
            {
                object data;

                if (_sessionData.TryGetValue(name, out data))
                {
                    value = (T) data;
                    result = true;
                }
                else
                {
                    value = default(T);
                }
            }
            catch
            {
                value = default(T);
            }

            return result;
        }

        public virtual void Set<T>(string name, T value)
        {
            _sessionData[name] = value;
        }

        public virtual void Clear(string name)
        {
            _sessionData[name] = null;
            _sessionData.Remove(name);
        }

        public virtual string SessionId
        {
            get { return Get<SessionToken>(SessionTokenSlot).SessionId; }
        }

        public virtual string SiteId
        {
            get { return Get<SessionToken>(SessionTokenSlot).SiteId; }
        }

        public virtual string CultureCode
        {
            get { return Get<SessionToken>(SessionTokenSlot).CultureCode; }
        }

        public virtual string GcLocale
        {
            get
            {
                SiteInfo siteInfo;
                return TryGetSiteInfo(out siteInfo) ? siteInfo.GcLocale : CultureCode.Replace('-', '_');
            }
        }

        public virtual string LanguageCode
        {
            get { return Get<SessionToken>(SessionTokenSlot).LanguageCode; }
        }

        public virtual string CountryCode
        {
            get { return Get<SessionToken>(SessionTokenSlot).CountryCode; }
        }

        public virtual string CurrencyCode
        {
            get { return Get<SessionToken>(SessionTokenSlot).CurrencyCode; }
        }

        public virtual string CompanyId
        {
            get { return Get<SiteInfo>(SiteInfoSlot).CompanyId; }
        }


    public virtual string McId
        {
            get { return Get<string>(McIdSlot); }
        }

        public virtual string IcId
        {
            get { return Get<string>(IcIdSlot); }
        }

        public virtual string Firstname
        {
            get
            {
                SecurityToken securityToken;
                return TryGet(SecurityTokenSlot, out securityToken)
                           ? securityToken.Firstname
                           : "";
            }
        }

        public virtual string Lastname
        {
            get
            {
                SecurityToken securityToken;
                return TryGet(SecurityTokenSlot, out securityToken)
                           ? securityToken.Lastname
                           : "";
            }
        }

        public virtual bool TryGetSiteInfo(out SiteInfo siteInfo)
        {
            return TryGet(SiteInfoSlot, out siteInfo);
        }

        private static bool TryGetSiteInfo(HttpContext context, out SiteInfo siteInfo)
        {
            string siteId;
            string cultureCode;

            if (!context.TryGetSiteId(out siteId) || !context.TryGetCultureCode(siteId, out cultureCode))
            {
                siteInfo = null;
                return false;
            }

            var siteConfiguration = ConfigLoader.Get<SiteConfig>();
            return siteConfiguration.TryGetSiteInfo(new SiteCultureInfo(siteId,
                (string)context.Items[MarketPlaceParameter], cultureCode, ""), out siteInfo);
        }


        public virtual string NameOnProfile
        {
            // TODO: will this work???
            get { return Get<string>(NameOnProfileSlot); }
        }

        public virtual bool IsAuthenticated
        {
            get
            {
                SecurityToken securityToken;
                return TryGet(SecurityTokenSlot, out securityToken);
            }
        }

        public Dictionary<string, string> PersistentProperties { get; set; }

        public void SetPersistentProperty(string name, string value)
        {
            if (PersistentProperties == null)
                PersistentProperties = new Dictionary<string, string>();
            PersistentProperties[name] = value;
        }

        public void SetPersistentProperties(Dictionary<string, string> properties)
        {
            if (PersistentProperties == null)
                PersistentProperties = new Dictionary<string, string>();
            foreach (var pair in properties)
                PersistentProperties[pair.Key] = pair.Value;
        }

        public string GetPersistentProperty(string name)
        {
            if (PersistentProperties != null && PersistentProperties.ContainsKey(name))
                return PersistentProperties[name];
            return String.Empty;
        }

        public void RemovePersistentProperty(string name)
        {
            if (PersistentProperties != null && PersistentProperties.ContainsKey(name))
                PersistentProperties.Remove(name);
        }

        public AfterAddToCartOption GetSessionAfterAddToCartOption()
        {
            var opt = GetPersistentProperty(AfterAddToCartOptionSessionName);
            if (string.IsNullOrEmpty(opt))
                return AfterAddToCartOption.GoToShoppingCart;
            return (AfterAddToCartOption)int.Parse(opt);
        }

        public void SetSessionAfterAddToCartOption(AfterAddToCartOption option)
        {
            SetPersistentProperty(AfterAddToCartOptionSessionName, ((int)option).ToString(CultureInfo.InvariantCulture));
        }


        #region IDisposable Members

        public void Dispose()
        {
            _sessionData = null;
        }

        #endregion

        #region Local Members

        private static string PreSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static void RequestParmToCookie(HttpContext context, WebSession webSession, string parmName, string slotName)
        {
            if (context.Request[parmName] == null)
                return;

            var parmValue = context.Request[parmName];

            if (context.Request.Cookies[parmName] == null ||
                !context.Request.Cookies[parmName].ToString().Equals(parmValue, StringComparison.InvariantCultureIgnoreCase))
            {
                var cookie = new HttpCookie(parmName, parmValue) { HttpOnly = true };
                context.Response.Cookies.Set(cookie);
            }

            if (webSession != null)
                webSession.Set(slotName, parmValue);
        }

        #endregion
    }
}