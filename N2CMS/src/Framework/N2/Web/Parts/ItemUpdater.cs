using System.Collections.Specialized;
using N2.Definitions;
using N2.Edit;
using N2.Engine;
using N2.Edit.Versioning;
using N2.Integrity;

namespace N2.Web.Parts
{
    /// <summary>
    /// ajax service used by inplace editors
    /// written by Stefan Weber
    /// </summary>
	[Service(typeof(IAjaxService))]
    public class ItemUpdater : PartsModifyingAjaxService
    {
        public ItemUpdater(Navigator navigator, IIntegrityManager integrity, IVersionManager versions, ContentVersionRepository versionRepository, IDefinitionManager definitionManager = null)
            : base(navigator, integrity, versions, versionRepository, definitionManager)
        {
        }

        public override string Name
        {
            get { return "update"; }
        }

        protected override bool UpdateItem(NameValueCollection request, ContentItem item)
        {
            string property = request["property"];
            string value = request["value"];

            // TODO check if property exists?

            // check if changed
            var old = (string)item[property];
            if (old == value) return false;
            
            // perform actual change
            item[property] = value;
            return true;
        }
    }
}