using N2.Edit;

// add Digital River items to the N2 toolbar
// see ToolbarPluginDisplay.cs for rendering

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Admin.Toolbars
{
    [ToolbarPlugin("DRCC", "DRCommandConsole", "https://gc.digitalriver.com",
         ToolbarArea.Options, "_blank", "~/Content/img/icons/DRLogo16.png", 1001,
         ToolTip = "Digital River | Command Console",
         GlobalResourceClassName = "Toolbar")]
    class DigitalRiverCCToolbar
    {
        // a dummy - TODO create a SSO with DR tools
    }

	//[ToolbarPlugin("BizNav", "DRBizNav", "https://biznav.digitalriver.com",
	//     ToolbarArea.Options, "_blank", "~/Content/img/icons/DRLogo16.png", 1002,
	//     ToolTip = "Digital River | Business Navigator",
	//     GlobalResourceClassName = "Toolbar")]
	//class DigitalRiverBizNavToolbar
	//{
	//    // a dummy - TODO create a SSO with DR tools
	//}
}
