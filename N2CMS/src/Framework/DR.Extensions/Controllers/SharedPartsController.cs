using N2.Models;
using N2.Web;

namespace N2.Controllers
{
	/// <summary>
	/// This controller will handle parts deriving from AbstractItem which are not 
	/// defined by another controller [Controls(typeof(MyPart))]. 
	/// </summary>
	[Controls(typeof(PartModelBase))]
	public class SharedPartsController : TemplatesControllerBase<PartModelBase>
	{
	}
}