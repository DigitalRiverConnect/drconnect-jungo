using System;
using System.Web.Caching;
using N2.Persistence;

namespace N2.Web.UI
{
    public class ContentCacheDependency : CacheDependency
    {
        IPersister persister;

        public ContentCacheDependency(IPersister persister)
        {
            this.persister = persister;
            persister.ItemMoved += persister_ItemInvalidated;
            persister.ItemSaved += persister_ItemInvalidated;
            persister.ItemDeleted += persister_ItemInvalidated;
            persister.ItemCopied += persister_ItemInvalidated;
            persister.FlushCache += persister_ItemInvalidated;
        }

        protected override void DependencyDispose()
        {
            persister.ItemMoved -= persister_ItemInvalidated;
            persister.ItemSaved -= persister_ItemInvalidated;
            persister.ItemDeleted -= persister_ItemInvalidated;
            persister.ItemCopied -= persister_ItemInvalidated;
            persister.FlushCache -= persister_ItemInvalidated;
        }

        void persister_ItemInvalidated(object sender, EventArgs e)
        {
            NotifyDependencyChanged(sender, e);
        }
    }
}
