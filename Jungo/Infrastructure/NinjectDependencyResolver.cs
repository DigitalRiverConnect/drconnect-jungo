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
using Ninject;

namespace Jungo.Infrastructure
{
    public class NinjectDependencyResolver: IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        #region Implementation of IDependencyResolver

        public object Get(Type type)
        {
            return _kernel.Get(type);
        }

        public object Get(Type type, string name)
        {
            return _kernel.Get(type, name);
        }

        public object TryGet(Type type)
        {
            return _kernel.TryGet(type);
        }

        public object TryGet(Type type, string name)
        {
            return _kernel.TryGet(type, name);
        }

        public IEnumerable<object> GetAll(Type type)
        {
            return _kernel.GetAll(type);
        }

        public T Get<T>()
        {
            return _kernel.Get<T>();
        }

        public T Get<T>(string name)
        {
            return _kernel.Get<T>(name);
        }

        public T TryGet<T>()
        {
            return _kernel.TryGet<T>();
        }

        public T TryGet<T>(string name)
        {
            return _kernel.TryGet<T>(name);
        }

        public IEnumerable<T> GetAll<T>()
        {
            return _kernel.GetAll<T>();
        }

        #endregion
    }
}
