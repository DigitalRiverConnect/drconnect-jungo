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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Attributes;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Cache;
using ReflectionMagic;
using DependencyResolver = Jungo.Infrastructure.DependencyResolver;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ActionFilters
{
    public class DynamicActionResultAttribute : ActionFilterAttribute
    {
        private readonly ICache<List<JsonField>> _propertyCache;

        public DynamicActionResultAttribute()
        {
            _propertyCache =
                DependencyResolver.Current.Get<ICacheFactory>().GetCache<List<JsonField>>("BaseControllerJsonFields");
        }
        
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewBase = filterContext.Result as ViewResultBase;

            if (viewBase != null)
            {
                var controller = filterContext.Controller;

                if (IsJson(filterContext.HttpContext))
                    filterContext.Result = Dynamic(controller, viewBase.ViewName, viewBase.ViewData.Model,
                                                   (c, bag, v, m) => new
                                                                         {
                                                                             properties = bag,
                                                                             html = ToPartialViewString(c, v, m)
                                                                         });
                else if (IsModelJson(filterContext.HttpContext))
                    filterContext.Result = Dynamic(controller, viewBase.ViewName, viewBase.ViewData.Model,
                                                   (c, bag, v, m) => new
                                                                         {
                                                                             properties = bag,
                                                                             model = m
                                                                         });
            }
        }

        protected virtual ActionResult Dynamic(ControllerBase controller, string viewName, object model, Func<ControllerBase, Dictionary<string, object>, string, object, object> json)
        {
            var propertyBag = new Dictionary<string, object>();
            
            if (model != null)
            {
                List<JsonField> properties;
                var cacheKey = model.GetType().FullName;

                if (!_propertyCache.TryGet(cacheKey, out properties))
                {
                    properties = new List<JsonField>();
                    properties.AddRange(model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(property => GetCustomAttribute(property, typeof(JsonPropertyAttribute)) != null)
                        .Select(property => new JsonField(property, (JsonPropertyAttribute)GetCustomAttribute(property, typeof(JsonPropertyAttribute))))
                        .ToList());
                    properties.AddRange(model.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                        .Where(field => GetCustomAttribute(field, typeof(JsonPropertyAttribute)) != null)
                        .Select(field => new JsonField(field, (JsonPropertyAttribute)GetCustomAttribute(field, typeof(JsonPropertyAttribute))))
                        .ToList());

                    _propertyCache.Add(cacheKey, properties);
                }

                properties.ForEach(property => propertyBag.Add(property.Name, property.GetValue(model)));
            }

            return controller.AsDynamic().Json(json(controller, propertyBag, viewName, model), JsonRequestBehavior.AllowGet);
        }

        protected virtual bool IsJson(HttpContextBase context)
        {
            return context.Request.AcceptTypes != null &&
                   context.Request.AcceptTypes.Contains("application/dr-json", StringComparer.InvariantCultureIgnoreCase);
        }

        protected virtual bool IsModelJson(HttpContextBase context)
        {
            return context.Request.AcceptTypes != null &&
                   context.Request.AcceptTypes.Contains("application/dr-model-json", StringComparer.InvariantCultureIgnoreCase);
        }

        private static string ToPartialViewString(ControllerBase controller, string view, object model)
        {
            if (string.IsNullOrEmpty(view))
                view = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, view);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }


        [Serializable]
        private class JsonField
        {
            public JsonField(PropertyInfo property, JsonPropertyAttribute attribute)
            {
                Property = property;
                Name = !string.IsNullOrWhiteSpace(attribute.Name) ? attribute.Name : property.Name;
            }

            public JsonField(FieldInfo field, JsonPropertyAttribute attribute)
            {
                Field = field;
                Name = !string.IsNullOrWhiteSpace(attribute.Name) ? attribute.Name : field.Name;
            }

            private PropertyInfo Property { get; set; }
            private FieldInfo Field { get; set; }

            public string Name { get; private set; }

            public object GetValue(object model)
            {
                if (Property != null)
                    return Property.GetValue(model, null);
                return Field != null ? Field.GetValue(model) : null;
            }
        }
    }
}
