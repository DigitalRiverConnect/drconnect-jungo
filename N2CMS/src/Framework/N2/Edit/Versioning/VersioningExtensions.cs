using System;
using System.Collections.Generic;
using System.Linq;
using N2.Definitions;
using N2.Edit.Workflow;
using N2.Engine;
using N2.Persistence;

namespace N2.Edit.Versioning
{
	public static class VersioningExtensions
	{
		public static ContentItem CloneForVersioningRecursive(this ContentItem item, StateChanger stateChanger = null, bool asPreviousVersion = true, DateTime? stamp = null)
		{
            if (stamp == null)
                stamp = Utility.CurrentTime().AddSeconds(-1); // hmm, time could elapse while cloning, better use a fixed date, e.g. on the statechanger

			ContentItem clone = item.Clone(false);
			if (stateChanger != null)
			{
				if (item.State == ContentState.Published && asPreviousVersion)
					stateChanger.ChangeTo(clone, ContentState.Unpublished);
				else if (item.State != ContentState.Unpublished || asPreviousVersion == false)
					stateChanger.ChangeTo(clone, ContentState.Draft);
			}
            clone.Updated = stamp.Value;
			clone.Parent = null;
			clone.AncestralTrail = "/";
			clone.VersionOf = item.VersionOf.Value ?? item;

			CopyAutoImplementedProperties(item, clone);

			foreach (var child in item.Children.Where(c => !c.IsPage))
			{
				var childClone = child.CloneForVersioningRecursive(stateChanger, asPreviousVersion, stamp);
				childClone.AddTo(clone);
			}

			return clone;
		}

		private static void CopyAutoImplementedProperties(ContentItem source, ContentItem destination)
		{
			foreach (var property in source.GetContentType().GetProperties().Where(pi => pi.IsInterceptable()))
			{
				destination[property.Name] = TryClone(source[property.Name]);
			}
		}

		private static object TryClone(object value)
		{
			if (value == null)
				// pass on null
				return null;

			if (value is ContentItem)
				return value;

			var type = value.GetType();
			if (!type.IsClass)
				// pass on value types
				return value;

			if (value is ICloneable)
				// clone clonable
				return (value as ICloneable).Clone();

			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(List<>))
				{
					// create new generic lists
					var ctor = type.GetConstructor(new [] { typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments()[0]) });
					if (ctor != null)
						return ctor.Invoke(new [] { value });
				}
			}

			// accept the rest
			return value;
		}

		public static ContentItem FindPartVersion(this ContentItem parent, ContentItem part)
		{
			if (part.ID == parent.VersionOf.ID)
				return parent;
			if (part.VersionOf.HasValue && part.VersionOf.ID == parent.VersionOf.ID)
				return parent;
			if (parent.ID == 0 && parent.GetVersionKey() == part.GetVersionKey())
				return parent;

			foreach (var child in parent.Children)
			{
				var grandChild = child.FindPartVersion(part);
				if (grandChild != null)
					return grandChild;
			}
			return null;
		}

		public static void SetVersionKey(this ContentItem item, string key)
		{
			item["VersionKey"] = key;
		}

		public static string GetVersionKey(this ContentItem item)
		{
			return item["VersionKey"] as string;
		}

		public static ContentItem FindDescendantByVersionKey(this ContentItem parent, string key)
		{
			if (string.IsNullOrEmpty(key))
				return null;

			var match = Find.EnumerateChildren(parent, includeSelf: true, useMasterVersion: false)
				.Where(d =>
				{
					var versionKey = d.GetVersionKey();
					return key.Equals(versionKey);
				}).FirstOrDefault();
			return match;
		}

	    /// <summary>Publishes the given version.</summary>
	    /// <param name="versionManager"></param>
	    /// <param name="versionToPublish">The version to publish.</param>
	    /// <returns>The published (master) version.</returns>
	    /// TODO this is only used in Publish below, but not in the command!
        public static ContentItem MakeMasterVersion(this IVersionManager versionManager, ContentItem versionToPublish)
		{
			if (!versionToPublish.VersionOf.HasValue)
				return versionToPublish;

			var master = versionToPublish.VersionOf;
			versionManager.ReplaceVersion(master, versionToPublish, versionToPublish.VersionOf.Value.State == ContentState.Published);
			return master;
		}

		[Obsolete]
		public static bool IsVersionable(this ContentItem item)
		{
			return !item.GetContentType()
				.GetCustomAttributes(typeof(VersionableAttribute), true)
				.OfType<VersionableAttribute>()
				.Any(va => va.Versionable == AllowVersions.No);
		}
		
		[Obsolete]
		public static void SchedulePublishing(this ContentItem previewedItem, DateTime publishDate, IEngine engine)
		{
			MarkForFuturePublishing(engine.Resolve<StateChanger>(), previewedItem, publishDate);
			engine.Persister.Save(previewedItem);
		}

		[Obsolete]
		public static void MarkForFuturePublishing(Workflow.StateChanger changer, ContentItem item, DateTime futureDate)
		{
			if (!item.VersionOf.HasValue)
				item.Published = futureDate;
			else
				item["FuturePublishDate"] = futureDate;
			changer.ChangeTo(item, ContentState.Waiting);
		}

        /// <summary>
        /// Used by PublishScheduledAction and the Publish Button in the Curtain
        /// </summary>
        /// <param name="versionManager"></param>
        /// <param name="persister"></param>
        /// <param name="previewedItem"></param>
        /// <returns></returns>
		public static ContentItem Publish(this IVersionManager versionManager, IPersister persister, ContentItem previewedItem)
        {
            //if (previewedItem.State == ContentState.Published)
            //    return previewedItem; // nothing to do

            if (!previewedItem.IsPage)
                throw new ArgumentException("Publish requires item to be a page");

            if (previewedItem.VersionOf.HasValue)
			{
                // is versioned
				previewedItem = versionManager.MakeMasterVersion(previewedItem);
				persister.Save(previewedItem);
			}

            var stateChanger = Context.Current.Resolve<StateChanger>();
            stateChanger.ChangeTo(previewedItem, ContentState.Published);
            persister.Save(previewedItem);
			return previewedItem;
		}

        /// <summary>
        /// Used by Versions-Dialog  -> previewedItem.VersionOf.HasValue = TRUE ?
        /// </summary>
        /// <param name="versionManager"></param>
        /// <param name="persister"></param>
        /// <param name="item">The item to be replaced (Master Version)</param>
        /// <param name="versionIndex">Index of the version to be published</param>
        public static void PublishVersion(this IVersionManager versionManager, IPersister persister, ContentItem item, int versionIndex)
        {
            if (!item.IsPage)
                throw new ArgumentException("PublishVersion requires item to be a page");

            if (item.VersionIndex != versionIndex)
            {
                // unpublish/restore
                ContentItem itemToPublish = versionManager.GetVersion(item, versionIndex);
                bool storeCurrent = item.State == ContentState.Published || itemToPublish.State == ContentState.Unpublished;
                versionManager.ReplaceVersion(item, itemToPublish, storeCurrent); // returns old version
            }

            Publish(versionManager, persister, item);
        }

	    /// <summary>
	    /// Used by Edit.aspx
	    /// </summary>
	    /// <param name="persister"></param>
	    /// <param name="item"></param>
	    public static void Unpublish(IPersister persister, ContentItem item)
        {
            if (!item.IsPage)
                throw new ArgumentException("Unpublish requires item to be page");

            var stateChanger = Context.Current.Resolve<StateChanger>();
            stateChanger.ChangeTo(item, ContentState.Unpublished);
            persister.Save(item);
        }
	}
}
