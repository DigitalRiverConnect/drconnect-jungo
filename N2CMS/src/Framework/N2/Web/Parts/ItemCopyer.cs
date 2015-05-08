using System.Collections.Specialized;
using N2.Definitions;
using N2.Edit;
using N2.Engine;
using N2.Edit.Versioning;
using System;
using N2.Integrity;

namespace N2.Web.Parts
{
	[Service(typeof(IAjaxService))]
    public class ItemCopyer : PartsModifyingAjaxService
	{
        public ItemCopyer(Navigator navigator, IIntegrityManager integrity, IVersionManager versions, ContentVersionRepository versionRepository, IDefinitionManager definitionManager = null)
            : base(navigator, integrity, versions, versionRepository, definitionManager)
	    {
	    }

	    public override string Name
		{
			get { return "copy"; }
		}

        protected override bool UpdateItem(NameValueCollection request, ContentItem item)
	    {
            item = item.Clone(true);
            item.Name = null;
            item.ZoneName = request["zone"];
            foreach (var child in Find.EnumerateChildren(item, true, false))
                child.SetVersionKey(Guid.NewGuid().ToString());

            int newIndex;
            var parent = GetParentAndIndex(request, Page, out newIndex);
            if (parent == null)
                throw new Exception("could not locate target of action");

            InsertItem(parent, item, newIndex);

            return true;
	    }

		protected override void ValidateLocation(ContentItem item, ContentItem parent)
		{
			var ex = integrity.GetCopyException(item, parent);
			if (ex != null)
				throw ex;
		}
	}
}