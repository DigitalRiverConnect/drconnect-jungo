using System.Web.UI.WebControls;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
	[PartDefinition("FAQ Item",
		Description = "A question with answer.",
		SortOrder = 130,
		IconUrl = "~/Content/img/icons/information.png")]
	[RestrictParents(typeof (FaqPage))]
	[AllowedZones("Questions")]
	[WithEditableTitle("Question", 90, Focus = false)]
	public class Faq : PartModelBase
	{
        [EditableFreeTextArea(title: "Answer", sortOrder: 100, TextMode = TextBoxMode.MultiLine)]
		public virtual string Answer
		{
			get { return (string) (GetDetail("Answer") ?? string.Empty); }
			set { SetDetail("Answer", value, string.Empty); }
		}
	}
}