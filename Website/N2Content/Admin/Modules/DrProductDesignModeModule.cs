using System.Collections.Generic;
using N2.Management.Api;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Admin.Modules
{
    [ManagementModule]
    public class DrProductDesignModeModule : ManagementModuleBase
    {
        public override IEnumerable<string> ScriptIncludes
        {
            get
            {
                yield return "~/Scripts/admin/customN2Toolbar.js";
            }
        }
    }
}
