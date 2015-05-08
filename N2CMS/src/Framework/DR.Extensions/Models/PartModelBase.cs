using N2.Definitions;
using N2.Integrity;

namespace N2.Models
{
	/// <summary>
	/// A base class for item parts in the Nimbus project.
	/// </summary>
    [RestrictParents(typeof(IPage), typeof(IPart))]
    public abstract class PartModelBase : ContentBase, IPart
	{
	}
}