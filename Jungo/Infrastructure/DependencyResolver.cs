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
//  06/21/2012  ALiu           Created
//  

using System;

namespace Jungo.Infrastructure
{
    public static class DependencyResolver
    {
        private static IDependencyResolver _dependencyResolver;

        public static void Register(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public static IDependencyResolver Current
        {
            get
            {
                if (_dependencyResolver == null)
                    throw  new Exception("No IDependencyResolver registered");
                return _dependencyResolver;
            }
        }

        public static bool Initialized
        {
            get { return _dependencyResolver != null; }
        }
    }
}
