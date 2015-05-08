//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: 2009/08/15 17:40:01 $
// Modified by: $Author: ALiu $
// Revision: $Revision: 1.1 $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  02/10/2012  ALiu           Created
//  

using System;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session
{
    public class SecurityToken
    {
        public string Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public int TokenExpiresIn { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string Ip { get; set; }
        public string LiveConnectClientId { get; set; }
        public string LiveConnectScopes { get; set; }
    }
}
