using N2.Edit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Admin.Toolbars
{
    [ToolbarPlugin("Prod Mode: Design", "DRProductMode", "javascript:void(0)",
         ToolbarArea.Options, "_self", "~/Content/img/icons/Package.png", 1002,
         ToolTip = "Digital River | Product Mode",
         IconClass = "n2-icon-gift",
         GlobalResourceClassName = "Toolbar", CustomController = "DR.ProductDesignModeController",
         ClientAction = "clicked()")]
    public class DrProductDesignModeToolbar
    {
    }
}
