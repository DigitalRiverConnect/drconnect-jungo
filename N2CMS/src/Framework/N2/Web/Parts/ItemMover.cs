using System;
using System.Collections.Specialized;
using N2.Definitions;
using N2.Edit;
using N2.Edit.Versioning;
using N2.Engine;
using N2.Integrity;

namespace N2.Web.Parts
{
	[Service(typeof(IAjaxService))]
	public class ItemMover : PartsModifyingAjaxService
    {
        public ItemMover(Navigator navigator, IIntegrityManager integrity, IVersionManager versions, ContentVersionRepository versionRepository, IDefinitionManager definitionManager = null)
            : base(navigator, integrity, versions, versionRepository, definitionManager) 
	    {
	    }

	    public override string Name
        {
            get { return "move"; }
        }

        protected override bool UpdateItem(NameValueCollection request, ContentItem item)
	    {
	        item.ZoneName = request["zone"];

            int newIndex;
            var parent = GetParentAndIndex(request, Page, out newIndex);
            if (parent == null)
            {
                parent = GetParentAndIndex(request, origPage, out newIndex);
                throw new Exception("could not locate target of action");
            }
            InsertItem(parent, item, newIndex);

            return true;
	    }

	    protected override void ValidateLocation(ContentItem item, ContentItem parent)
		{
			var ex = integrity.GetMoveException(item, parent);
			if (ex != null)
				throw ex;
		}
    }
}