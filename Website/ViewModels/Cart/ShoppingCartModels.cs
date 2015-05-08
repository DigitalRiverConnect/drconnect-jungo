using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using DataAnnotationsExtensions;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Attributes;
using Jungo.Models.ShopperApi.Common;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart
{
    public class AddProductModel
    {
        [Required(ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "AddProductModel_ProductIdMissing")]
        public string ProductId { get; set; }

        public string Quantity { get; set; }

        public string OfferId { get; set; }
    }

    public class MiniShoppingCartViewModel
	{
		[JsonProperty(Name = "count")]
		public int Count { get; set; }

		[JsonProperty(Name = "subTotal")]
		public decimal SubTotal { get; set; }

		[JsonProperty(Name = "discount")]
		public decimal Discount { get; set; }

		[JsonProperty(Name = "shippingAndHandling")]
		public decimal ShippingAndHandling { get; set; }

		[JsonProperty(Name = "tax")]
		public decimal Tax { get; set; }

		[JsonProperty(Name = "total")]
		public decimal Total { get; set; }

		public MiniShoppingCart ShoppingCart { get; set; }
	}


	public class MiniShoppingCart
	{
		public MiniShoppingCart(Jungo.Models.ShopperApi.Cart.Cart cart)
		{
            Count = cart.TotalItemsInCart;
            SubTotal = cart.Pricing.Subtotal;
            Discount = cart.Pricing.Discount;
            ShippingAndHandling = cart.Pricing.ShippingAndHandling;
            Tax = cart.Pricing.Tax;
            Total = cart.Pricing.OrderTotal;

			LineItems = cart.LineItems
				.LineItem
				.Select(lineItem => new MiniShoppingCartLineItem
				                    	{
				                    		LineItemId = lineItem.Id.ToString(CultureInfo.InvariantCulture),
				                    		Description = lineItem.Product.ShortDescription,
				                    		Image = lineItem.Product.ProductImage,
				                    		Quantity = lineItem.Quantity,
                                            UnitPrice = lineItem.Pricing.ListPrice,
				                    		SubTotal = lineItem.Pricing.SalePriceWithQuantity
				                    	});
		}

		public int Count { get; set; }

		public MoneyAmount SubTotal { get; set; }

        public MoneyAmount Discount { get; set; }

        public MoneyAmount ShippingAndHandling { get; set; }

        public MoneyAmount Tax { get; set; }

        public MoneyAmount Total { get; set; }

		public IEnumerable<MiniShoppingCartLineItem> LineItems { get; set; }

		public class MiniShoppingCartLineItem
		{
			public string LineItemId { get; set; }
			public string Description { get; set; }
			public string Image { get; set; }
			public int Quantity { get; set; }
            public MoneyAmount UnitPrice { get; set; }
            public MoneyAmount SubTotal { get; set; }
		}
	}


	public class UpdateShippingModel
	{
		public string CarrierCode { get; set; }
	}

    public class AddCouponModel
    {
        [Required(ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "AddCouponModel_CouponCodeMissing")]
        [RegularExpression(@"^.{1,20}$", ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "AddCouponModel_CouponCodeInvalid")]
        public string CouponCode { get; set; }
    }

    public class UpdateLineItemModel
    {
        [Required(ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "UpdateLineItemModel_LineItemIdMissing")]
        public string LineItemId { get; set; }

        [Required(ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "UpdateLineItemModel_QuantityMissing")]
        [Min(0, ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "UpdateLineItemModel_QuantityTooSmall")]
        public int Quantity { get; set; }
    }

    public class ChangeVariationModel
    {
        [Required(ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "ChangeVariationModel_LineItemIdMissing")]
        public string LineItemId { get; set; }

        [Required(ErrorMessageResourceType = typeof (Resources.Messages),
            ErrorMessageResourceName = "ChangeVariationModel_ProductIdMissing")]
        public string ProductId { get; set; }
    }
}
