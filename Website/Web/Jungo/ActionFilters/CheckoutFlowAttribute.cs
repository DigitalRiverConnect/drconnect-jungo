using System.Web;
using System.Web.Mvc;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ActionFilters
{
    public class CheckoutFlowAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            HttpContext.Current.Items["IsInCheckoutWorkflow"] = true;
        }
    }
}
