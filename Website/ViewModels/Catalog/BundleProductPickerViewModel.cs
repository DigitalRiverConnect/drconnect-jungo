//using DigitalRiver.CloudLink.Commerce.Api.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class BundleProductPickerViewModel
    {
        public string OfferId { get; set; }
        public string OfferName { get; set; }
        public string OfferInstanceId { get; set; }
        public OfferBundleGroupViewModel[] OriginalBundleGroups { get; set; }
        public OfferBundleGroupViewModel[] FullyMandatoryBundleGroups { get; set; }
        public OfferBundleGroupViewModel[] PartiallyMandatoryBundleGroups { get; set; }
        public OfferBundleGroupViewModel[] OptionalBundleGroups { get; set; }
    }

    public class BundleProductPickerAddToCartErrors
    {
        public bool HasGetCustomBundleOfferException { get; set; }
        public CustomBundleGroupAddToCartError[] GroupErrors { get; set; }
        public string[] ProductIdsNotInAnyGroup { get; set; }
        public string AddProductToCartError { get; set; }
    }

    public class BundleProductPickerAddToCartRequest
    {
        public string ProductId { get; set; }
        public string OfferId { get; set; }
        public string[] BuyProductIds { get; set; }
    }

    public class BundleProductPickerAddToCartResponse
    {
        public BundleProductPickerAddToCartErrors Errors { get; set; }
        public BundleProductPickerViewModel Offer { get; set; }
        public bool AddedToCart { get; set; }
    }

    public class CustomBundleGroupAddToCartError
    {
        public string BundleGroupInstanceId { get; set; }
        public bool TooMany { get; set; }
        public bool NotEnough { get; set; }
    }

    public class OfferBundleGroupViewModel
    {
        public string BundleGroupInstanceId { get; set; }
        public string BundleGroupName { get; set; }
        public int MinProductQuantity { get; set; }
        public int MaxProductQuantity { get; set; }
        public BundleOfferProductViewModel[] Products { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class BundleOfferProductViewModel
    {
        //public BundleOfferProductInfo Product { get; set; }
        //public ProductPricing Pricing { get; set; }
        public string ProductDetailLink { get; set; }
        public string ProductSalesPitch { get; set; }
        public string ProductOfferImage { get; set; }
        public int DisplayOrder { get; set; }
        public int AvailableQuantity { get; set; }
        public ProductMediaViewModel[] DetailMedia { get; set; }
    }
}
