using System;
using System.Linq;
using System.Collections.Generic;
using N2.Collections;
using N2.Edit.Versioning;
using N2.Engine;
using N2.Persistence;
using N2.Web;

namespace N2.Edit
{
	[Service(typeof(ITreeSorter))]
	public class TreeSorter : ITreeSorter
	{
		IPersister persister;
		IEditManager editManager;
		IWebContext webContext;
        private IVersionManager versionMaker;


        public TreeSorter(IPersister persister, IEditManager editManager, IWebContext webContext, IVersionManager versionMaker)
		{
			this.persister = persister;
			this.editManager = editManager;
			this.webContext = webContext;
            this.versionMaker = versionMaker;
		}

		#region ITreeSorter Members

		public void MoveUp(ContentItem item)
		{
			if (item.Parent != null)
			{
				ItemFilter filter = editManager.GetEditorFilter(webContext.User);
				IList<ContentItem> siblings = item.Parent.Children;
				IList<ContentItem> filtered = new ItemList(siblings, filter);

				int index = filtered.IndexOf(item);
				if (index > 0)
				{
					MoveTo(item, NodePosition.Before, filtered[index - 1]);
				}
			}
		}

		public void MoveDown(ContentItem item)
		{
			if (item.Parent != null)
			{
				ItemFilter filter = editManager.GetEditorFilter(webContext.User);
				IList<ContentItem> siblings = item.Parent.Children;
				IList<ContentItem> filtered = new ItemList(siblings, filter);
				int index = filtered.IndexOf(item);
				if (index + 1 < filtered.Count)
				{
					MoveTo(item, NodePosition.After, filtered[index + 1]);
				}
			}
		}

		public void MoveTo(ContentItem item, ContentItem parent)
		{
			if (item.Parent == parent)
			{
				// move it last
				item.AddTo(null);
				item.AddTo(parent);
			}
			else if (item.Parent == null || !parent.Children.Contains(item))
				item.AddTo(parent);

		    UpdateSortOrderAndSave(parent);
		}

		public void MoveTo(ContentItem item, ContentItem parent, int index)
		{
			if (item.Parent != parent || !parent.Children.Contains(item))
				item.AddTo(parent);
			else if (parent.Children.Contains(item) && parent.Children.Last() != item)
			{
				item.AddTo(null);
				item.AddTo(parent);
			}

			IList<ContentItem> siblings = parent.Children;
			Utility.MoveToIndex(siblings, item, index);
            UpdateSortOrderAndSave(parent);
   		}

		public void MoveTo(ContentItem item, int index)
		{
			IList<ContentItem> siblings = item.Parent.Children;
			Utility.MoveToIndex(siblings, item, index);
            UpdateSortOrderAndSave(item.Parent);
		}

	    private void UpdateSortOrderAndSave(ContentItem parent)
	    {
	        IEnumerable<ContentItem> siblings = parent.Children;
	        var changes = 0;

	        using (var tx = persister.Repository.BeginTransaction())
	        {
	            foreach (ContentItem updatedItem in Utility.UpdateSortOrder(siblings))
	            {
	                if (updatedItem.ID > 0)
	                {
                        persister.Repository.SaveOrUpdate(updatedItem);
	                    changes++;
	                }
                    else
                    {
                        Logger.ErrorFormat("skip save for {0}", updatedItem); // TODO
                    }
	            }
                if (changes == 0 && versionMaker != null)
	            {
                    var page = Find.ClosestPage(parent);

	                if (page.State == ContentState.Draft)
	                {
                        var pageVersion = page.VersionOf.HasValue ? page
                            : versionMaker.AddVersion(page, asPreviousVersion: false);

                        // TODO
                        //if (pageVersion != null && pageVersion.SavedBy != state.User.Identity.Name)
                        //    pageVersion.SavedBy = state.User.Identity.Name;

                        versionMaker.UpdateVersion(pageVersion);

                        Logger.ErrorFormat("save draft of {0}", page); // TODO
                        //persister.Repository.SaveOrUpdate(page);
	                }
	            }
	            tx.Commit();
	        }
	    }

	    public void MoveTo(ContentItem item, NodePosition position, ContentItem relativeTo)
		{
            if (relativeTo == null) throw new ArgumentNullException("item");
            if (relativeTo == null) throw new ArgumentNullException("relativeTo");
            if (relativeTo.Parent == null) throw new ArgumentException("The supplied item '" + relativeTo + "' has no parent to add to.", "relativeTo");
            
			if (item.Parent == null 
				|| item.Parent != relativeTo.Parent
				|| !item.Parent.Children.Contains(item))
				item.AddTo(relativeTo.Parent);

			IList<ContentItem> siblings = item.Parent.Children;
			
			int itemIndex = siblings.IndexOf(item);
			int relativeToIndex = siblings.IndexOf(relativeTo);
			
            if(itemIndex < 0)
            {
                if(position == NodePosition.Before)
                    siblings.Insert(relativeToIndex, item);
                else
                    siblings.Insert(relativeToIndex + 1, item);
            }
		    else if(itemIndex < relativeToIndex && position == NodePosition.Before)
				MoveTo(item, relativeToIndex - 1);
			else if (itemIndex > relativeToIndex && position == NodePosition.After)
				MoveTo(item, relativeToIndex + 1);
			else
				MoveTo(item, relativeToIndex);
		}
		#endregion
	}
}
