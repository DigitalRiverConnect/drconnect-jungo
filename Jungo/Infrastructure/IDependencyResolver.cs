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
//  06/12/2012  ALiu           Created
//  

using System;
using System.Collections.Generic;

namespace Jungo.Infrastructure
{
    public interface IDependencyResolver
    {
        object Get(Type type);

        object Get(Type type, string name);

        object TryGet(Type type);

        object TryGet(Type type, string name);

        IEnumerable<object> GetAll(Type type);

        T Get<T>();

        T Get<T>(string name);

        T TryGet<T>();

        T TryGet<T>(string name);

        IEnumerable<T> GetAll<T>();
    }
}
