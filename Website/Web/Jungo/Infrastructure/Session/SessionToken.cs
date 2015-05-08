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
//  01/31/2012  HGodinez           Created
// 

using System;
using System.Runtime.Serialization;
using System.Text;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session
{
    [DataContract]
    public class SessionToken
    {
        private const string Prefix = "V1";
        private const char Colon = '~';

        private SessionToken()
        {
        }

        public SessionToken(string siteId, string cultureCode, string countryCode, string currencyCode, string externalId, string sessionId)
        {
            SiteId = siteId;
            CultureCode = cultureCode;
            CountryCode = countryCode;
            CurrencyCode = currencyCode;
            ExternalId = externalId;
            SessionId = sessionId;
            LanguageCode = ParseCultureCode(CultureCode);
            PreSessionId = sessionId;
        }

        public void ResetSessionId(string sessionId)
        {
            SessionId = sessionId;
        }

        [DataMember(Name = "SiteId")]
        public string SiteId { get; private set; }

        [DataMember(Name = "CultureCode")]
        public string CultureCode { get; private set; }

        [DataMember(Name = "LanguageCode")]
        public string LanguageCode { get; private set; }

        [DataMember(Name = "CountryCode")]
        public string CountryCode { get; private set; }

        [DataMember(Name = "CurrencyCode")]
        public string CurrencyCode { get; private set; }

        [DataMember(Name = "ExternalId")]
        public string ExternalId { get; private set; }

        [DataMember(Name = "SessionId")]
        public string SessionId { get; private set; }

        [DataMember(Name = "PreSessionId")]
        public string PreSessionId { get; private set; }


        public static bool TryParse(string value, out SessionToken sessionToken)
        {
            bool result;

            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    var parts = value.Split(new[] { Colon }, 9);
                    if (parts.Length == 9 && parts[0] == Prefix)
                    {
                        sessionToken = new SessionToken
                                           {
                                               SiteId = parts[1] == "null" ? null : parts[1],
                                               CultureCode = parts[2] == "null" ? null : parts[2],
                                               LanguageCode = parts[3] == "null" ? null : parts[3],
                                               CountryCode = parts[4] == "null" ? null : parts[4],
                                               CurrencyCode = parts[5] == "null" ? null : parts[5],
                                               ExternalId = parts[6] == "null" ? null : parts[6],
                                               SessionId = parts[7] == "null" ? null : parts[7],
                                               PreSessionId = parts[8] == "null" ? null : parts[8]
                                           };
                        result = true;
                    }
                    else
                    {
                        sessionToken = null;
                        result = false;
                    }
                }
                catch (Exception)
                {
                    sessionToken = null;
                    result = false;
                }
            }
            else
            {
                sessionToken = null;
                result = false;
            }


            return result;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(Prefix).Append(Colon);
            result.Append(SiteId ?? "null").Append(Colon);
            result.Append(CultureCode ?? "null").Append(Colon);
            result.Append(LanguageCode ?? "null").Append(Colon);
            result.Append(CountryCode ?? "null").Append(Colon);
            result.Append(CurrencyCode ?? "null").Append(Colon);
            result.Append(ExternalId ?? "null").Append(Colon);
            result.Append(SessionId ?? "null").Append(Colon);
            result.Append(PreSessionId ?? "null");
            return result.ToString();
        }

        #region Local Members

        private static string ParseCultureCode(string cultureCode)
        {
            string result = null;

            if (cultureCode != null)
            {
                result = cultureCode.IndexOf('-') > -1
                    ? cultureCode.Substring(0, cultureCode.IndexOf('-'))
                    : cultureCode;
            }

            return result;
        }

        #endregion

    }
}
