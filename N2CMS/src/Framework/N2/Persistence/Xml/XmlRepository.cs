using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using N2.Configuration;
using N2.Edit.Versioning;
using N2.Engine;

namespace N2.Persistence.Xml
{
    [Service(typeof(IRepository<>), Configuration = "xml", Replaces = typeof(NH.NHRepository<>))]
    public class XmlRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        // memory cache
        protected IDictionary<object, TEntity> Database = new ConcurrentDictionary<object, TEntity>();
        private static Logger<XmlRepository<TEntity>> _logger;
        private readonly string databaseDir;
        private readonly Type _tEntityType;
        private readonly PropertyInfo _tEntityIdProperty;
        private readonly PropertyInfo _tEntityNameProperty;

        public XmlRepository(DatabaseSection config, ConnectionStringsSection connectionStrings)
        {
            _tEntityType = typeof (TEntity);
            _tEntityIdProperty = _tEntityType.GetProperty("ID");
            _tEntityNameProperty = _tEntityType.GetProperty("Name");

            if (config != null && !string.IsNullOrEmpty(config.ConnectionStringName))
            {
                ConnectionStringSettings css = connectionStrings.ConnectionStrings[config.ConnectionStringName];
                if (css == null)
                    throw new ConfigurationErrorsException("Could not find the connection string named '" +
                                                           config.ConnectionStringName +
                                                           "' that was defined in the n2/database configuration section.");

                databaseDir = css.ConnectionString;
            }

            if (string.IsNullOrEmpty(databaseDir))
                databaseDir = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/ContentItemsXml");

            if (string.IsNullOrEmpty(databaseDir))
                databaseDir = Path.Combine(Path.GetTempPath(), "xmlrepotest" + GetHashCode()); // ensure different paths for replication tests

            if (!Directory.Exists(DataDirectoryPhysical))
                Directory.CreateDirectory(DataDirectoryPhysical);
            else if (config == null && connectionStrings == null)
            {
                InternalDeleteFiles("*"); // assume unit test and delete all files
            }

            _logger.Debug("NEW XmlRepository " + typeof(TEntity).FullName + " in " + databaseDir);
        }

        public string DataDirectoryPhysical
        {
            get
            {
                return databaseDir;
            }
        }

        public virtual TEntity Get(object id)
        {
            if (id is Int32 && (int)id == 0)
                return null;
            if (!Database.ContainsKey(id))
                return null;
            return Database[id];
        }

        public virtual IEnumerable<TEntity> Find(string propertyName, object value)
        {
            return Find((IParameter)Parameter.Equal(propertyName, value));
        }

        public IEnumerable<TEntity> Find(params Parameter[] propertyValuesToMatchAll)
        {
            if (propertyValuesToMatchAll.Length == 0)
            {
                foreach (var item in Database.Values) 
                    yield return item;
            }
            else
            {
                foreach (var item in Database.Values)
                    if (propertyValuesToMatchAll.All(condition => condition.IsMatch(item)))
                        yield return item;
            }
        }

        public virtual IEnumerable<TEntity> Find(IParameter parameters)
        {
            var result = from w in Database
                   let x = w.Value
                   where parameters.IsMatch(x)
                   select x;

            var pc = parameters as ParameterCollection;
            if (pc != null)
            {
                if (pc.Order != null && pc.Order.HasValue)
                {
                    result = pc.Order.Descending ? result.OrderByDescending(e => Utility.Evaluate(e, pc.Order.Property))
                                                 : result.OrderBy(e => Utility.Evaluate(e, pc.Order.Property));
                }

                if (pc.Range != null)
                {
                    if (pc.Range.Skip > 0)
						result = result.Skip(pc.Range.Skip);
					if (pc.Range.Take > 0)
                        result = result.Take(pc.Range.Take);
                }
            }

            return result;
        }

        public IEnumerable<IDictionary<string, object>> Select(IParameter parameters, params string[] properties)
        {
            return Find(parameters).Select(e => properties.ToDictionary(p => p, p => Utility.GetProperty(e, p)));
        }

        public virtual void Delete(TEntity entity)
        {
            InternalDeleteFiles(GetPath(entity, true));

            // need to create a list here to avoid mutation errors
            var list = Database.Where(x => x.Value == entity).Select(x => x.Key).ToList();
            foreach (var toRemove in list)
            {
                _logger.Info(string.Format("Deleting item {0} \"{1}\"", 
                    _tEntityIdProperty.GetValue(entity, null), 
                    _tEntityNameProperty.GetValue(entity, null)));
                Database.Remove(toRemove);
            }

        }

        // delete existing file/s
        protected void InternalDeleteFiles(string name)
        {
            try
            {
                foreach (var f in Directory.GetFiles(DataDirectoryPhysical, name))
                { 
                    _logger.Info(string.Format("Deleting file {0}", f));
                    File.Delete(f);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("InternalDeleteFiles " + name + " failed " + ex.Message, ex);
            }
        }

        public virtual void SaveOrUpdate(TEntity item)
        {
            if (typeof(TEntity) == typeof(ContentVersion))
            {
                //TODO: Make XmlRepository handle versions
                throw new NotImplementedException();
            }
            
            CreateFile(item);
        }

        private void CreateFile(TEntity item)
        {
            var s = new System.Xml.Serialization.XmlSerializer(item.GetType());
            using (var fs = File.CreateText(GetPath(item)))
            {
                s.Serialize(fs, item);
                fs.Flush();
                fs.Close();
            }
        }

        public virtual string GetPath(TEntity item, bool wild = false)
        {
            return Path.Combine(DataDirectoryPhysical, String.Format("t{0}-{1}.xml", _tEntityType.Name, _tEntityIdProperty.GetValue(item, null)));
        }

        public bool Exists()
        {
            return Count() > 0;
        }

        public virtual long Count()
        {
            return Database.Count;
        }

        public long Count(IParameter parameters)
        {
            return Find(parameters).Count();
        }

        public void Flush()
        {
            if (typeof(TEntity) == typeof(ContentVersion))
            {
                //TODO: Make XmlRepository handle versions
                throw new NotImplementedException();
            }
        }

        private ITransaction transaction;
        public ITransaction BeginTransaction()
        {
            return transaction = new FakeTransaction();
        }

        public ITransaction GetTransaction()
        {
            return transaction;
        }

        // NOTE this class is actually not destroyed when Dispose is called, it just ends the transaction
        public virtual void Dispose()
        {          
            if (transaction != null)
                transaction.Dispose();
        }
    }
}
