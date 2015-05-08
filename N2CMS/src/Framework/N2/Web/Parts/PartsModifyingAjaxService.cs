using System;
using System.Collections.Specialized;
using System.Linq;
using N2.Definitions;
using N2.Edit;
using N2.Edit.Versioning;
using N2.Integrity;

namespace N2.Web.Parts
{
    /// <summary>
    /// Ajax service concerned with modifying a page (edits)
    /// </summary>
    public abstract class PartsModifyingAjaxService : PartsAjaxService
    {
        protected readonly Navigator navigator;        
        protected readonly IIntegrityManager integrity;
        protected readonly IVersionManager versions;
        protected readonly ContentVersionRepository versionRepository;
        protected readonly IDefinitionManager definitionManager;


        protected PartsModifyingAjaxService(Navigator navigator, IIntegrityManager integrity, IVersionManager versions, ContentVersionRepository versionRepository, IDefinitionManager definitionManager)
        {
            this.navigator = navigator;
            this.integrity = integrity;
            this.versions = versions;
            this.versionRepository = versionRepository;
            this.definitionManager = definitionManager;
        }

        // find item (and optional version) from request values (compare WebExtension.GetEditableWrapper)
        public static ContentItem FindItem(IVersionManager versions, ContentVersionRepository versionRepository, Navigator navigator, NameValueCollection request)
        {
            var item = navigator.Navigate(request[PathData.ItemQueryKey]);
            return versionRepository.ParseVersion(request[PathData.VersionIndexQueryKey], 
                                                  request[PathData.VersionKeyQueryKey], item) ?? item;
        }

        // find the target item and index
        protected ContentItem GetParentAndIndex(NameValueCollection request, ContentItem page, out int newIndex)
        {
            var beforeItem = PartsExtensions.GetBeforeItem(navigator, request, page);
            ContentItem parent;
            if (beforeItem != null)
            {
                parent = beforeItem.Parent;
                newIndex = parent.Children.IndexOf(beforeItem);
            }
            else
            {
                parent = PartsExtensions.GetBelowItem(navigator, request, page);
                newIndex = parent == null ? 0 : parent.Children.Count;
            }
            return parent;
        }

        protected void InsertItem(ContentItem parent, ContentItem item, int newIndex)
        {
            ValidateLocation(item, parent);
            Utility.Insert(item, parent, newIndex);
            Utility.UpdateSortOrder(parent.Children);
        }

        protected ContentItem origPage = null;
        protected ContentItem Page = null;
            
        public override NameValueCollection HandleRequest(NameValueCollection request)
        {
            bool isNew = false;
            var response = new NameValueCollection();
            try
            {
                // locate the item to be modified
                var original = FindItem(versions, versionRepository, navigator, request);

                var disc = request["discriminator"];
                if (definitionManager != null && !string.IsNullOrEmpty(disc) && !definitionManager.GetDefinition(original).Discriminator.Equals(disc))
                    throw new Exception("unexpected part type for " + original);
#if DEBUG2                
                origPage = Find.ClosestPage(original);
                Dump(origPage);
                Logger.DebugFormat("AJAX found item {0} on page {1}", original, origPage);
#endif
                // get or create a draft to work with
                var path = PartsExtensions.EnsureDraft(original, versions, versionRepository, out isNew);
                var item = path.CurrentItem;
                if (definitionManager != null && !string.IsNullOrEmpty(disc) && !definitionManager.GetDefinition(item).Discriminator.Equals(disc))
                    throw new Exception("unexpected part type for " + item);

                Page = path.CurrentPage;
#if DEBUG2
                    if (isNew || page != origPage)
                        Logger.DebugFormat("AJAX changing item {0} on page {1}", item, page);
#endif
                // perform action on item
                if (UpdateItem(request, item))
                {
                    // ensure proper user name 
                    if (Principal != null)
                        Page.SavedBy = item.SavedBy = Principal.Identity.Name;
                    versionRepository.Save(Page);
                    Logger.DebugFormat("AJAX saved page {0} by {1}", Page, Page.SavedBy);
#if DEBUG
                    Dump(Page);
#endif
                }
                //response["redirect"] = isNew ? (string) Page.Url.ToUrl().SetQueryParameter("edit", "drag") : "#";
                response["redirect"] = (string)Page.Url.ToUrl().SetQueryParameter("edit", "drag").SetQueryParameter("random", Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                if (isNew)
                {
                    Logger.ErrorFormat("TODO Rollback Draft {0} after {1}", Page, ex.Message);
                    // TODO
                }

                throw;
            }
            return response;
        }

        protected void Dump(ContentItem item)
        {
            var history = versions.GetVersionsOf(item).Select(v => string.Format("{0} '{1}' [{2}] u:{3} p:{4} e:{5} by:{6}", 
                v.VersionIndex, v.Title, v.State, v.Updated, v.Published, v.Expires, v.SavedBy));
            foreach (var h in history)
            {
                Logger.Debug(h);
            }
        }


        protected abstract bool UpdateItem(NameValueCollection request, ContentItem item);

        protected virtual void ValidateLocation(ContentItem item, ContentItem parent)
        {
            throw new NotImplementedException();
        }
    }
}