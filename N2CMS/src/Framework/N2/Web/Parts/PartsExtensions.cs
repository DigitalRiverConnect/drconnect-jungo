using System.Linq;
using N2.Details;
using System.Web.Routing;
using N2.Edit.Versioning;
using System.Collections.Specialized;
using N2.Engine;

namespace N2.Web.Parts
{
	public static class PartsExtensions
	{
		static Engine.Logger<object> logger;

		public static T LoadEmbeddedPart<T>(this ContentItem item, string keyPrefix) where T : ContentItem, new()
		{
			var part = new T();
			var collection = item.GetDetailCollection(keyPrefix, false);
			if(collection != null)
			{
				foreach (var cd in collection.Details)
				{
					var name = cd.Name.Substring(keyPrefix.Length + 1);
					if (cd.ValueTypeKey == ContentDetail.TypeKeys.LinkType)
						// avoid retrieving item from database
						part[name] = cd.LinkedItem;
					else
						part[name] = cd.Value;
				}
			}
			return part;
		}

		public static void StoreEmbeddedPart(this ContentItem item, string keyPrefix, ContentItem part)
		{
			if (part == null)
			{
				foreach (var detail in item.Details.Where(d => d.Name.StartsWith(keyPrefix + ".")).ToList())
					item.Details.Remove(detail);
				return;
			}

			DetailCollection collection = item.GetDetailCollection(keyPrefix, true);
			foreach (var propertyName in ContentItem.KnownProperties.WritablePartProperties)
			{
				SetDetail(item, collection, keyPrefix + "." + propertyName, part[propertyName]);
			}
			foreach (var cd in part.Details)
			{
				SetDetail(item, collection, keyPrefix + "." + cd.Name, cd.Value);
			}
		}

		private static void SetDetail(ContentItem item, DetailCollection collection, string key, object value)
		{
			ContentDetail cd = collection.Details.FirstOrDefault(d => d.Name == key);
			if (value != null)
			{
				if (cd == null)
				{
					cd = ContentDetail.New(key, value);
					cd.EnclosingItem = item;
					cd.EnclosingCollection = collection;
					collection.Details.Add(cd);
				}
				else
					cd.Value = value;
			}
			else if (cd != null)
				collection.Details.Remove(cd);
		}

		public static T LoadEmbeddedObject<T>(this ContentItem item, string keyPrefix) where T : new()
		{
			var entity = new T();
			var collection = item.GetDetailCollection(keyPrefix, false);
			if (collection != null)
			{
				foreach (var cd in collection.Details)
				{
					if (!Utility.TrySetProperty(entity, cd.Name.Substring(keyPrefix.Length + 1), cd.Value))
						logger.WarnFormat("Unable to assign property '{0}' from {1} with prefix '{2}'", cd.Name, item, keyPrefix);
				}
			}
			return entity;
		}

		public static void StoreEmbeddedObject(this ContentItem item, string keyPrefix, object entity)
		{
			DetailCollection collection = item.GetDetailCollection(keyPrefix, true);
			StoreObjectOnDetails(item, keyPrefix, entity, collection);
		}

		private static void StoreObjectOnDetails(ContentItem item, string keyPrefix, object entity, DetailCollection collection)
		{
			foreach (var kvp in new RouteValueDictionary(entity))
			{
				if (ContentDetail.GetAssociatedPropertyName(kvp.Value) == "Value")
					StoreObjectOnDetails(item, keyPrefix + "." + kvp.Key, kvp.Value, collection);
				else
					SetDetail(item, collection, keyPrefix + "." + kvp.Key, kvp.Value);
			}
		}

        // locate an item by VersionKey on a page (used for DRAFTs)

		public static ContentItem GetBeforeItem(Edit.Navigator navigator, NameValueCollection request, ContentItem page)
		{
		    ContentItem item = page.FindDescendantByVersionKey(request["beforeVersionKey"]);
		    if (item != null) return item;
			
            ContentItem beforeItem = navigator.Navigate(request["before"]);
            return beforeItem == null ? null : page.FindPartVersion(beforeItem);
			}

		public static ContentItem GetBelowItem(Edit.Navigator navigator, NameValueCollection request, ContentItem page)
			{
		    ContentItem item = page.FindDescendantByVersionKey(request["belowVersionKey"]);
		    if (item != null) return item;

		    var parent = navigator.Navigate(request["below"]);
            return parent == null ? null : page.FindPartVersion(parent);
		}

        // see also UseDraftCommand.cs (should use it here?)
        public static PathData EnsureDraft(ContentItem item, IVersionManager versions, ContentVersionRepository versionRepository, out bool isNew)
        {
            isNew = false;

            // find containing page
            var page = Find.ClosestPage(item);
            if (versions.IsVersionable(page) && page.State != ContentState.Draft)
            {
                // current version is not draft, see if there are any drafts
                // page is not versioned, add a new version
                page = versions.AddVersion(page, asPreviousVersion: false);

                // find part to be modified on new page version
                item = page.FindPartVersion(item);
                isNew = true;
            }

            return new PathData(page, item);
        }
	}
}
