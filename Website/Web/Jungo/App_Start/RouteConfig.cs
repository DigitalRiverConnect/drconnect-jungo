using System.Web.Mvc;
using System.Web.Routing;
using N2.Web.Mvc;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("Styles/{*pathInfo}");

            routes.IgnoreRoute("{*sosa}", new { font = @"(.*/)?sosa.(/.*)?" });
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.([iI][cC][oO]|[gG][iI][fF])(/.*)?" });

            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.map/{*pathInfo}");


            routes.MapRoute(
                "N2Diagnostics",
                "Diagnostic/{action}/{id}",
                new { controller = "Diagnostic", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
                "RestrictedAdmin",
                "Restricted/Admin/{action}/{id}",
                new { controller = "Admin", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Sitemap1",
                "sitemap.xml",
                new { controller = "Sitemap", action = "Index" }
            );

            routes.MapRoute(
                "Sitemap2",
                "{siteId}/sitemap.xml",
                new { controller = "Sitemap", action = "Index" }
            );

            routes.MapRoute(
                "Sitemap3",
                "{siteId}/{cultureCode}/sitemap.xml",
                new { controller = "Sitemap", action = "Index" }
            );

            // n2cms content route
            routes.MapContentRoute("CMS", N2.Context.Current, true);


            // Image Picker Controller: GetFiles method
            routes.MapRoute(
                "ImagePickerUploadFiles",
                "ImagePicker/UploadFile",
                new { controller = "ImagePicker", action = "UploadFile" } // Parameter defaults
            );

            // Image Picker Controller: GetFiles method

            routes.MapRoute(
                "ImagePickerGetFiles",
                "ImagePicker/GetFiles",
                new { controller = "ImagePicker", action = "GetFiles" } // Parameter defaults
            );

            // Image Picker Controller: Search method
            routes.MapRoute(
                "ImagePickerImageSearch",
                "ImagePicker/Search",
                new { controller = "ImagePicker", action = "Search" } // Parameter defaults
            );

            // Image Picker controller: Index method
            routes.MapRoute(
                "ImagePickerIndex",
                "ImagePicker/{id}",
                new { controller = "ImagePicker", action = "Index" }
                );

            routes.MapRoute(
                "CategoryPicker", // Route name
                "CategoryPicker/{id}/{categoryId}", // URL with parameters
                new { controller = "CategoryPicker", action = "Index", categoryId = UrlParameter.Optional } // Parameter defaults
                );

            routes.MapRoute(
                "ProductPickerProductSearch", // Route name
                "ProductPicker/SearchProducts/{keywords}/{page}/{pageSize}", // URL with parameters
                new { controller = "ProductPicker", action = "SearchProducts", page = UrlParameter.Optional, pageSize = UrlParameter.Optional } // Parameter defaults
                );

            routes.MapRoute(
                "ProductPicker", // Route name
                "ProductPicker/{id}/{mode}/{selectedItems}", // URL with parameters
                new { controller = "ProductPicker", action = "Index", selectedItems = UrlParameter.Optional } // Parameter defaults
                );

            routes.MapRoute(
                "ContentPicker", // Route name
                "ContentPicker/{id}/{startPageId}", // URL with parameters
                new { controller = "ContentPicker", action = "Index" } // Parameter defaults
                );

            //routes.MapRoute(
            //    "SiteServices", // Route name
            //    "Services/{siteId}/{cultureCode}/{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            //    );

            routes.MapRoute(
                "Account", // Route name
                "Account/{action}/{id}", // URL with parameters
                new { controller = "Account", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );

            routes.MapRoute(
                "SiteServices", // Route name
                "{siteId}/{cultureCode}/{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );
        }
    }
}