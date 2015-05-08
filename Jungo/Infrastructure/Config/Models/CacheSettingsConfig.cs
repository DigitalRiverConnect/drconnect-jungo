//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  11/14/2013  Alex Liu       Created
// 

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jungo.Infrastructure.Config.Models
{
    [Serializable]
    public class CacheSettingsConfig
    {
        public int DefaultTtl { get; set; }
        public Item[] Settings { get; set; }

        public int GetTtl(string name)
        {
            var item = Settings == null ? null : Settings.FirstOrDefault(i => i.IsMatch(name));
            var result = item == null ? DefaultTtl : item.Ttl;
            return result;
        }

        [Serializable]
        public class Item
        {
            public string NameRegex { get; set; }
            public int Ttl { get; set; }

            private Regex _regex;
            public bool IsMatch(string name)
            {
                if (_regex == null)
                    _regex = new Regex(NameRegex,
                        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                return _regex.IsMatch(name);
            }
        }
    }
}
