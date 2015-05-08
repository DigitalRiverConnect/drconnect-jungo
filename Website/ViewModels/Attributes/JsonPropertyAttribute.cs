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
//  3/29/2013   EHornbostel    Created
// 

using System;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Attributes
{
    [Serializable]
    public class JsonPropertyAttribute : Attribute
    {
        public string Name;
    }
}
