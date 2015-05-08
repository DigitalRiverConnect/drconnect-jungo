using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using N2.Definitions;
using N2.Definitions.Runtime;
using N2.Definitions.Static;
using N2.Details;
using N2.Engine;
using N2.Models;
using N2.Persistence;
using N2.Plugin;
using N2.Web;

namespace N2.Services
{
	[Service(typeof(ITemplateProvider))]
	public class ContentPartTemplateProvider : ITemplateProvider 
	{
		private readonly Logger<ContentPartTemplateProvider> _logger;
		private readonly DefinitionBuilder _builder;
		private readonly ContentActivator _activator;
		private readonly IProvider<HttpContextBase> _httpContextProvider;
		private readonly ConnectionMonitor _connection;
	    private readonly IDictionary<string, int> partNameToID = new ConcurrentDictionary<string, int>();
		//private readonly VirtualPathProvider _vpp;
		//private readonly IRepository<ContentItem> _repository;
		private readonly IHost _host;
		bool _rebuild = true;

		public ContentPartTemplateProvider(
			ContentActivator activator, 
			DefinitionBuilder builder, 
			IProvider<HttpContextBase> httpContextProvider, 
			//IProvider<VirtualPathProvider> vppProvider, 
			//IRepository<ContentItem> repository, 
			ConnectionMonitor connection,
			IHost host)
		{
			_activator = activator;
			_builder = builder;
			_httpContextProvider = httpContextProvider;
			_connection = connection;
			//_vpp = vppProvider.Get();
			//_repository = repository;
			_host = host;
		}


		private IPersister _persister = null;
		private IPersister Persister
		{
			get
			{
				// track changes to void cache
				if (_persister == null)
				{
					// lazy initialization to break cyclic IoC dependency on IPersister
					_persister = Context.Current.Persister;
					_connection.Online += delegate
					{
						_persister.ItemSaved += PersisterOnChanged;
						_persister.ItemDeleted += PersisterOnChanged;
						// needed? _persister.ItemMoved += PersisterOnChanged;
					};
					_connection.Offline += delegate
					{
						_persister.ItemSaved -= PersisterOnChanged;
						_persister.ItemDeleted -= PersisterOnChanged;
					};
				}
				return _persister;
			}
		}

		private void PersisterOnChanged(object sender, ItemEventArgs itemEventArgs)
		{
			if (itemEventArgs.AffectedItem is PartDefinitionPage)
			{
				_rebuild = true; // void cache
			}
		}

        public int GetIdForPartName(string name)
        {
            if (partNameToID.Count == 0)
                RegisterDynamicParts();

            int result;
            return !partNameToID.TryGetValue(name, out result) ? 0 : result;
        }

		public static string RemoveSpecialCharacters(string str)
		{
			var sb = new StringBuilder();
			foreach (char c in str.Where(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_'))
			{
				sb.Append(c);
			}
			return sb.ToString();
		}

		private ContentRegistration RegisterPart(PartDefinitionPage pd)
		{
			var def = DefinitionMap.Instance.GetOrCreateDefinition(typeof(ContentPart), pd.Title); // no extensions

			// registration - compare Dinamico views  @{ Content.Define(re => ... ) }
			var re = new ContentRegistration(def)
			    {
			        Definition = {TemplateKey = pd.Name, Description = pd.Description},
			        Context = {GlobalSortOffset = pd.PartSortOrder},
			        ContentType = def.ItemType,
			        Title = pd.Title,
			        IconUrl = pd.IconUrl
			    };

		    //if (!string.IsNullOrEmpty(pd.Definition))
			//{
			//    var jsd = new JavaScriptRegistration();
			//    jsd.Eval(re, pd.Definition);
			//}

			foreach (var attrDef in pd.Attributes)
			{
				AbstractEditableAttribute attr = null;
				var name = RemoveSpecialCharacters(attrDef.Title);
				switch (attrDef.PartType)
				{
					case PartDefinitionPage.AttributePart.AttributePartTypeEnum.RichText:
						attr = new EditableTextAttribute { TextMode = TextBoxMode.MultiLine, Name = name, Title = attrDef.Title };
						break;

					case PartDefinitionPage.AttributePart.AttributePartTypeEnum.Number:
						attr = new EditableNumberAttribute { Name = name, Title = attrDef.Title };
						break;

					case PartDefinitionPage.AttributePart.AttributePartTypeEnum.CheckBox:
						attr = new EditableCheckBoxAttribute { Name = name, Title = attrDef.Title };
						break;

					case PartDefinitionPage.AttributePart.AttributePartTypeEnum.Url:
						attr = new EditableUrlAttribute { Name = name, Title = attrDef.Title };
						break;

					case PartDefinitionPage.AttributePart.AttributePartTypeEnum.Image:
						attr = new EditableImageAttribute { Name = name, Title = attrDef.Title };
						break;

#if TODO // use a better mechanism to find available types/attrs
                    case PartDefinitionPage.AttributePart.AttributePartTypeEnum.Category:
                        attr = new EditableCategorySelectionAttribute("ID:" + name) { Name = name, Title = attrDef.Title };
                        break;

                    case PartDefinitionPage.AttributePart.AttributePartTypeEnum.Product:
                        attr = new EditableProductMultiSelectionAttribute("Id:" + name) { Name = name, Title = attrDef.Title };
                        break;
#endif
                    case PartDefinitionPage.AttributePart.AttributePartTypeEnum.Text:
                    default:
                        attr = new EditableTextAttribute { TextMode = TextBoxMode.SingleLine, Name = name, Title = attrDef.Title };
                        break;
                    //throw new ArgumentOutOfRangeException();
				}
				if (!string.IsNullOrEmpty(attrDef.DefaultValue))
					attr.DefaultValue = attrDef.DefaultValue;

				attr.HelpText = attrDef.HelpText;

				re.Add(attr);
			}

			re.IsDefined = true;
			re.Finalize();
			return re; 
		}

		private void AddRegistrationsBelow(ContentItem root, IList<ContentRegistration> list)
		{
			foreach (var child in root.Children)
			{
                if (child == null || string.IsNullOrEmpty(child.Name))
                    continue;

			    if (child is PartDefinitionPage && child.IsPublished())
			    {
			        list.Add(RegisterPart(child as PartDefinitionPage));
			        partNameToID[child.Name] = child.ID;
			    }
			    else if (child is FolderPage && child.IsPublished())
			        AddRegistrationsBelow(child, list);
			}
		}

		private IEnumerable<ItemDefinition> RegisterDynamicParts()
		{
			// Define registered parts
			var parts = new List<ContentRegistration>();
			var root = Persister.Get(_host.CurrentSite.RootItemID);
            partNameToID.Clear();
			AddRegistrationsBelow(root, parts);

			// Refine
			_builder.ExecuteRefiners(parts.Select(r => r.Definition).ToList());
			return parts.Select(registration => registration.Finalize());
		}

		private IEnumerable<ItemDefinition> GetTemplateDefinitions()
		{
			const string cacheKeyDefs = "PartDefinitionPageDefinitions";
			IEnumerable<ItemDefinition> definitions = null;

			var httpContext = _httpContextProvider.Get();
			if (httpContext != null)
			{
				try
				{
					httpContext.Request.GetType();
					definitions = httpContext.Cache[cacheKeyDefs] as IEnumerable<ItemDefinition>;
				}
				catch (Exception ex)
				{
					_logger.Warn("Trying to get templates with invalid context", ex);
					httpContext = null;
				}
			}

			if (definitions == null || _rebuild)
			{
				lock (this)
				{
					definitions = RegisterDynamicParts();

					// Cache
					//var files = registrations.SelectMany(p => p.Context.TouchedPaths).Distinct().ToList();
					//_logger.DebugFormat("Setting up cache dependency on {0} files", files.Count);

					//var cacheDependency = _vpp.GetCacheDependency(files.FirstOrDefault(), files, DateTime.UtcNow);
					if (httpContext != null)
					{
						httpContext.Cache.Remove(cacheKeyDefs);
						httpContext.Cache.Insert(cacheKeyDefs, definitions);
						// TODO add cache dependency on CMS content? Maybe not needed with _rebuild logic.

						//httpContext.Cache.Add(cacheKeyDefs, definitions,
						//                      cacheDependency,
						//                      System.Web.Caching.Cache.NoAbsoluteExpiration,
						//                      System.Web.Caching.Cache.NoSlidingExpiration,
						//                      CacheItemPriority.AboveNormal,
						//                      delegate { _logger.Debug("Razor template part changed"); }
						//    );
					}

					_rebuild = false;
				}
			}
			return definitions;
		}

		public IEnumerable<TemplateDefinition> GetTemplates(Type contentType)
		{
			return GetTemplateDefinitions().Where(d => d.ItemType == contentType).Select(d =>
			{
				var td = new TemplateDefinition
				{
					Definition = d,
					Description = d.Description,
					Name = d.TemplateKey,
					OriginalFactory = () => null,
					TemplateFactory = () => _activator.CreateInstance(d.ItemType, null, d.TemplateKey),
					TemplateUrl = null,
					Title = d.Title + '*',
                    ReplaceDefault = true // causes hiding of base class "ContentPart", see DefinitionManager.GetTemplates
				};
				return td;
			}).ToArray();
		}

		public TemplateDefinition GetTemplate(ContentItem item)
		{
			//var httpContext = _httpContextProvider.Get();
			//if (httpContext != null)
			//    if (RegistrationExtensions.GetRegistrationExpression(httpContext) != null)
			//        return null;

			var templateKey = item.TemplateKey;
			if (templateKey == null)
				return null;

			return GetTemplates(item.GetContentType()).Where(t => t.Name == templateKey).Select(t =>
			{
				t.OriginalFactory = t.TemplateFactory;
				t.TemplateFactory = () => item;
				return t;
			}).FirstOrDefault();
		}

		/// <summary>The order this template provider should be invoked, default 0.</summary>
		public int SortOrder { get; set; }
	}
}
