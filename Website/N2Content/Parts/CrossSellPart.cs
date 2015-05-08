using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
using N2;
using N2.Details;
using N2.Integrity;
using N2.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts
{
    [PartDefinition("API Cross Sell",
        Description = "Cross Sell Part to display other products tied to one or more other products on the page.",
        SortOrder = 200,
        IconUrl = "~/Content/img/icons/package.png")]
    [RestrictPageTypes(typeof(ProductPage))]
    [WithEditableTitle(Name = "Title")]
    public class CrossSellPart : PartModelBase
    {
        [EditablePopSelection(title: "Promotion ID", sortOrder: 100, HelpText = "Promotion ID for this cross sell promotion.", Required = true)]
        public virtual string PromotionId
        {
            get { return (string)GetDetail("PromotionId"); }
            set { SetDetail("PromotionId", value); }
        }
    }

    [PartDefinition("API Candy Rack",
        Description = "Cross Sell Part to display other products tied to one or more other products on the page.",
        SortOrder = 200,
        IconUrl = "~/Content/img/icons/package.png")]
    [RestrictPageTypes(typeof(ShoppingCartPage))]
    [WithEditableTitle(Name = "Title")]
    public class CandyRackPart : CrossSellPart
    {
        [EditablePopSelection(title: "Empty Cart Promotion ID", sortOrder: 105, HelpText = "Promotion ID for this candy rack when the cart is empty.", Required = true)]
        public virtual string EmptyCartPromotionId
        {
            get { return (string)GetDetail("EmptyCartPromotionId"); }
            set { SetDetail("EmptyCartPromotionId", value); }
        }

        [EditableNumber(Title = "Max number of items in the candy rack", SortOrder = 115, HelpTitle = "Optional", HelpText = "You can limit the allowed max number of products here",
            DefaultValue = 4, MinimumValue = "0", MaximumValue = "30", InvalidRangeText = "The maximum value is 30, minimum 0", Required = false)]
        public virtual int? MaxNumberOfProducts
        {
            get { return (int?)GetDetail("MaxNumberOfProducts"); }
            set { SetDetail("MaxNumberOfProducts", value); }
        }
    }

    [PartDefinition("API Cross Sell for Interstitial",
        Description = "Cross Sell Part for Interstitial to display other products tied to one or more other products on the page.",
        SortOrder = 210,
        IconUrl = "~/Content/img/icons/package.png")]
    [RestrictPageTypes(typeof(ShoppingCartInterstitialPage))]
    [WithEditableTitle(Name = "Title", Required = false)]
    public class CrossSellInterstitialPart : CrossSellPart
    {
        [StaticHtml(100)]
        public override string PromotionId
        {
            get { return @"<div style='color:black;font-weight:bold'>The general promotion ID for this page is defined on the <span style='color:blue;'>Start Page</span> for this site.</div>"; }
        }
    }
}
