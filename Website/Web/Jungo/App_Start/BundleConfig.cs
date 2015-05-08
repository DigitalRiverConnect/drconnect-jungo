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
//  06/06/2012  EHornbostel     Created

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Xml.Linq;
using N2.Interfaces;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs
{
    public class BundleConfig
    {
        private readonly IExternalWebLinkResolver _externalWebLinkResolver;
        private readonly bool _jsMapExists;
        private readonly bool _cssMapExists;
        private readonly ResourceMap _jsResourceMap;
        private readonly ResourceMap _cssResourceMap;
        private readonly XDocument _bundleXml;

        private class BundleDirectory
        {
            public string Path { get; set; }
            public string Pattern { get; set; }
            public bool IncludeSubdirectories { get; set; }
        }

        public BundleConfig(IExternalWebLinkResolver externalWebLinkResolver)
        {
            _externalWebLinkResolver = externalWebLinkResolver;

            var jsMapPath = HttpContext.Current.Server.MapPath("js_log.xml");
            var cssMapPath = HttpContext.Current.Server.MapPath("css_log.xml");

            _jsMapExists = File.Exists(jsMapPath);
            _cssMapExists = File.Exists(cssMapPath);

            _jsResourceMap = _jsMapExists ? new ResourceMap(jsMapPath) : null;
            _cssResourceMap = _cssMapExists ? new ResourceMap(cssMapPath) : null;

            _bundleXml = XDocument.Load(HttpContext.Current.Server.MapPath("Bundle.xml"));
        }

        // see http://www.asp.net/mvc/tutorials/mvc-4/bundling-and-minification
        public void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;

            // 3rd party libraries
            AddScriptBundle(bundles, "core");

            // Site javascript
            AddScriptBundle(bundles, "site_js");

            // Admin javascript
            AddScriptBundle(bundles, "admin_js");
           
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/coteries/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            // Project Related
            // CSS

            // Site
            AddCssBundle(bundles, "site_css");
            AddCssBundle(bundles, "site_css_subset_1");
            AddCssBundle(bundles, "site_css_pdp");
            AddCssBundle(bundles, "site_css_pcf");
            AddCssBundle(bundles, "site_css_grid");
            AddCssBundle(bundles, "site_css_responsive_parts");
            // Admin
            AddCssBundle(bundles, "admin_css");
            // IE workarounds
            AddCssBundle(bundles, "ie_css");

#if OPTIMIZE
            BundleTable.EnableOptimizations = true; // ignore debug setting in web.config
#endif
        }

        private void AddScriptBundle(BundleCollection bundles, string bundleName)
        {
            var mapReferenceName = string.Format("/{0}.min.js", bundleName);
            var cdnPath = _jsMapExists ? _jsResourceMap.MappedPath(mapReferenceName) : null;

            var bundleReference = string.Format("~/coteries/{0}", bundleName);
            var scriptBundle = string.IsNullOrEmpty(cdnPath) ?
                new ScriptBundle(bundleReference) :
                new ScriptBundle(bundleReference, _externalWebLinkResolver.GetPublicUrl(cdnPath));

            var coreFiles = GetBundleFiles(bundleName);
            var coreDirectories = GetBundleDirectories(bundleName);
            var bundle = scriptBundle.Include(coreFiles);

            foreach (var d in coreDirectories)
            {
                bundle.IncludeDirectory(d.Path, d.Pattern, d.IncludeSubdirectories);
            }

            bundles.Add(bundle);
        }

        private void AddCssBundle(BundleCollection bundles, string bundleName)
        {
            var mapReferenceName = string.Format("/{0}.min.css", bundleName);
            var cdnPath = _cssMapExists ? _cssResourceMap.MappedPath(mapReferenceName) : null;

            var bundleReference = string.Format("~/coteries/{0}", bundleName);
            var styleBundle = string.IsNullOrEmpty(cdnPath) ?
                new StyleBundle(bundleReference) :
                new StyleBundle(bundleReference, _externalWebLinkResolver.GetPublicUrl(cdnPath));

            var coreFiles = GetBundleFiles(bundleName);
            var coreDirectories = GetBundleDirectories(bundleName);
            var bundle = styleBundle.Include(coreFiles);

            foreach (var d in coreDirectories)
            {
                bundle.IncludeDirectory(d.Path, d.Pattern, d.IncludeSubdirectories);
            }

            bundles.Add(bundle);
        }

        private string[] GetBundleFiles(string bundleName)
        {
            var b = _bundleXml.Element("WebGrease")
                    .Elements("JsFileSet")
                    .FirstOrDefault(x => x.Attribute("name").Value.Equals(bundleName, StringComparison.InvariantCultureIgnoreCase)) ??
                    _bundleXml.Element("WebGrease")
                    .Elements("CssFileSet")
                    .FirstOrDefault(x => x.Attribute("name").Value.Equals(bundleName, StringComparison.InvariantCultureIgnoreCase));

            if(b == null)
                return new string[0];

            return b.Element("Inputs")
                .Elements("Input")
                .Where(x => x.Attribute("searchPattern") == null)
                .Select(x => string.Format("~/{0}", x.Value.Replace('\\', '/')))
                .ToArray();
        }

        private IEnumerable<BundleDirectory> GetBundleDirectories(string bundleName)
        {
            var b = _bundleXml.Element("WebGrease")
                   .Elements("JsFileSet")
                   .FirstOrDefault(x => x.Attribute("name").Value.Equals(bundleName, StringComparison.InvariantCultureIgnoreCase)) ??
                   _bundleXml.Element("WebGrease")
                   .Elements("CssFileSet")
                   .FirstOrDefault(x => x.Attribute("name").Value.Equals(bundleName, StringComparison.InvariantCultureIgnoreCase));

            if(b == null)
                return new BundleDirectory[0];

            return b.Element("Inputs")
                    .Elements("Input")
                    .Where(x => x.Attribute("searchPattern") != null)
                    .Select(x => new BundleDirectory
                    {
                        Path = string.Format("~/{0}", x.Value.Replace('\\', '/')),
                        Pattern = x.Attribute("searchPattern").Value,
                        IncludeSubdirectories = x.Attribute("searchOption") != null && x.Attribute("searchOption").Value.Equals("AllDirectories")
                    })
                    .ToArray();
        }
    }

    internal class ResourceMap
    {
        private readonly XDocument _document;

        public ResourceMap(string path)
        {
            _document = XDocument.Load(path);
        }

        public string MappedPath(string fileToMap)
        {
            try
            {
                var e = _document
                    .Element("RenamedFiles")
                    .Elements("File")
                    .FirstOrDefault(x => x.Element("Input").Value.Equals(fileToMap, StringComparison.InvariantCultureIgnoreCase));

                return e == null ? null : e.Element("Output").Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}