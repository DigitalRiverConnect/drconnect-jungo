using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jungo.Infrastructure.Cache;
using Newtonsoft.Json;

namespace Jungo.Infrastructure.Config
{
    public class JsonFileConfigLoader : IConfigLoader
    {
        public JsonFileConfigLoader(ICacheFactory cacheFactory, IConfigPathMapper configPathMapper)
        {
            _cache = cacheFactory.GetCache<string>("Config");
            _configPathMapper = configPathMapper;
            LocateEnvironment();
        }

        private readonly IConfigPathMapper _configPathMapper;
        private readonly ICache<string> _cache;
        private string _environment;
        private const string EnvironmentFile = "environment.txt";
        private string _configPath;
        private void LocateEnvironment()
        {
            var appPath = _configPathMapper.Map("~");
            var dirInfo = new DirectoryInfo(appPath);
            while (dirInfo != null && string.IsNullOrEmpty(_configPath))
            {
                var configDir = Path.Combine(dirInfo.FullName, "Config");
                if (Directory.Exists(configDir))
                    _configPath = configDir;
                else
                    dirInfo = Directory.GetParent(dirInfo.FullName);
            }
            if (string.IsNullOrEmpty(_configPath))
                throw new Exception("No Config folder in or up from application root");
            var envirFilename = Path.Combine(_configPath, EnvironmentFile);
            if (!File.Exists(envirFilename))
                throw new Exception("Missing " + envirFilename);
            _environment = File.ReadAllText(envirFilename).Trim();
            if (string.IsNullOrEmpty(_environment))
                throw new Exception(envirFilename + " is empty. Put the name of your current environment there, such as Development.");
        }

        #region IConfigLoader Members

        public T Get<T>() where T : class
        {
            string json;
            var cacheKey = typeof (T).Name;
            if (!_cache.TryGet(cacheKey, out json))
            {
                if (TryReadConfig(cacheKey, out json))
                    _cache.Add(cacheKey, json);
            }

            if (string.IsNullOrEmpty(json)) return null;

            var instance = JsonConvert.DeserializeObject<T>(json);
            if (instance == null)
                throw new Exception("The type " + typeof (T) + " could not be initialized.");

            return instance;
        }

        #endregion

        private static string NormalizeName(string name)
        {
            return name.EndsWith("Config") ? name.Remove(name.Length - 6) : name;
        }

        private bool TryReadConfig(string configName, out string value)
        {
            value = null;

            try
            {
                var jsonFile = NormalizeName(configName) + ".json";
                var pieces = _environment.Split('.');
                var searchPaths = new List<string>
                {
                    Path.Combine(_configPath, "Local", jsonFile),
                };
                for (var pi = pieces.Length; pi > 0; pi--)
                {
                    var pth = string.Join("\\", pieces, 0, pi);
                    searchPaths.Add(Path.Combine(_configPath, pth, jsonFile));
                }
                searchPaths.Add(Path.Combine(_configPath, jsonFile));

                var configFileInfo = searchPaths.Select(p => new FileInfo(p))
                    .FirstOrDefault(fi => fi != null && fi.Exists);

                if (configFileInfo == null)
                {
                    //todo: _logger.Debug("Not found {0} in {1}", configKey, string.Join(";", searchPaths));
                    return false;
                }
                using (var f = configFileInfo.OpenText())
                {
                    value = f.ReadToEnd();
                }

                //todo: _logger.Debug("Read configuration '{0}' for '{1}' from {2}", configKey.Name, configKey.ToString(), configFileInfo.FullName);

                return true;
            }
            catch (Exception e)
            {
                //todo: _logger.Warn(e, "Cannot resolve configuration file for {0}: {1}", configKey, e.Message);
            }

            return false;
        }
    }
}
