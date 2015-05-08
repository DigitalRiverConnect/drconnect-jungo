using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using DigitalRiver.CloudLink.Commerce.Api.Configuration;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Utils;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Extensions;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using Newtonsoft.Json;
using Jungo.Api;
using VariationsCollection = DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog.VariationsCollection;

namespace ViewModelBuilders.Catalog
{
    public interface IProductViewModelBuilder // for testing
    {
        Task<OffersViewModel> GetPromotion(string promotionName, Dictionary<string, string> attributes);
        Task<ProductDetailPageViewModel> GetProductDetail(Product product, SiteInfo siteInfo, bool skipStockAvailabilitycheck);
        Task<BundleProductPickerViewModel> GetBundleProductPickerViewModel(long productId, string offerId);
    }

    public class ProductViewModelBuilder : IProductViewModelBuilder
    {
        private readonly ICatalogApi _catalogApi;
        private readonly IRequestLogger _logger;

        // TODO: These should be configurable
        // These are made public solely for testing purposes!
        public const string PdpThumbnailImageAttributeName = "PDPThumbnailImage";
        public const string PdpLargeImageAttributeName = "PDPImageLarge";
        public const string PdpAltTextAttributeName = "PDPAltImageText";
        public const string PdpImageTypeAttributeName = "PDPImageType";
        public const string PdpImageTypeContentAttributeName = "PDPImageTypeContent";

        public ProductViewModelBuilder(ICatalogApi catalogApi, IRequestLogger logger)
        {
            _catalogApi = catalogApi;
            _logger = logger;
        }

        public async Task<ProductDetailPageViewModel> GetProductDetail(Product product, SiteInfo siteInfo, bool skipStockAvailabilitycheck)
        {
            if (product == null) return null;

            var displayProduct = product;

            if (product.ParentProduct != null)
            {
                product = await _catalogApi.GetProductAsync(product.ParentProduct).ConfigureAwait(false);
            }

            displayProduct = displayProduct.ParentProduct != null ? displayProduct : product.GetDisplayProduct(skipStockAvailabilitycheck);

            var formActionUri = new Uri(siteInfo.DrBuyNowFormAction);
            formActionUri = formActionUri.Query.Length > 0
                ? new Uri(formActionUri.AbsoluteUri + "?ThemeId=" + siteInfo.DrThemeId)
                : new Uri(formActionUri.AbsoluteUri + "&ThemeId=" + siteInfo.DrThemeId);

            var result = new ProductDetailPageViewModel(siteInfo)
            {
                Product = product,
                DisplayProduct = displayProduct,
                VariationOptions = GetVariationsByAttributes(product, product.Variations),
                SeoMetaDescription = product.LongDescription,
                BuyViewModel = new BuyViewModel
                {
                    SiteInfo = siteInfo,
                    ProductId = displayProduct.Id,
                    FormActionUri = formActionUri
                }
            };

            if (result.HasVariationOptions)
            {
                    
                result.VariationOptionsJson = JsonConvert.SerializeObject(result.VariationOptions);
                result.VariationLevels = result.VariationOptions.Levels
                    .Select(l => result.VariationOptions.FirstOrDefault(l))
                    .Where(vl => vl != null)
                    .ToArray();
            }

            result.SetPageTitle(result.Product.DisplayName);

            return result;
        }

        private VariationsCollection GetVariationsByAttributes(Product parentProduct, Variations variations)
        {
            VariationsCollection collection = null;

            if (variations != null)
            {
                var config = ConfigLoader.Get<ProductVariationsConfig>();
                var group = GetGroup(config, parentProduct, variations);

                if (group != null)
                {
                    // GUID changes on every call - use a value that is driven off the data
                    //var id = Guid.NewGuid().ToString("N");
                    var sb = new StringBuilder();
                    sb.Append(parentProduct.Id);
                    foreach (var p in variations.Product)
                    {
                        sb.Append('-');
                        sb.Append(p.Id);
                    }
                    var id = sb.ToString();

                    var result = GetVariationsByAttributes(variations, id, group.Variations, 0, new List<string>(), new List<string>());
                    if (result != null)
                    {
                        collection = new VariationsCollection
                                         {
                                             Id = id,
                                             Count = group.Variations.Length,
                                             Levels = @group.Variations.Select(v => v.Id + "_" + id).ToArray(),
                                             Variations = result
                                         };
                    }
                }
            }

            return collection;
        }

        private static VariationsCollection.Variation GetVariationsByAttributes(Variations variations, string id, IList<ProductVariationsConfig.Variation> groupVariations, int variationLevel, List<string> attributeNames, List<string> attributeValues)
        {
            if (groupVariations.Count == variationLevel)
                return null;

            var groupVariation = groupVariations[variationLevel];
            var variationProducts = FilterVariationProductsByAttributes(variations.Product.ToList(), 0, attributeNames, attributeValues);
            var values = variationProducts.Select(p => p.CustomAttributes.ValueByName<string>(groupVariation.AttributeName, null)).Distinct(StringComparer.InvariantCultureIgnoreCase);
            var options = new List<VariationsCollection.Option>();

            foreach (var value in values)
            {
                var option = CreateOption(variationProducts, variations, groupVariation, value);
                var attNames = new List<string>(attributeNames) { groupVariation.AttributeName };
                var attValues = new List<string>(attributeValues) { value };
                option.Variations = GetVariationsByAttributes(variations, id, groupVariations, variationLevel + 1, attNames, attValues);
                if (option.Variations == null)
                {
                    option.VariationProduct = GetProductByAttribute(variationProducts, groupVariation.AttributeName, value);
                }
                options.Add(option);
            }

            var result = new VariationsCollection.Variation
            {
                Id = groupVariation.Id + "_" + id,
                Name = groupVariation.Id,
                Options = options.ToArray()
            };

            return result;
        }

        private static List<Product> FilterVariationProductsByAttributes(List<Product> products, int attributeLevel, List<string> attributeNames, List<string> attributeValues)
        {
            if (attributeNames.Count == attributeLevel)
                return products;

            var result = products.Where(p => String.Equals(p.CustomAttributes.ValueByName<string>(attributeNames[attributeLevel], null),
                                             attributeValues[attributeLevel], StringComparison.InvariantCultureIgnoreCase)).ToList();

            return FilterVariationProductsByAttributes(result, attributeLevel + 1, attributeNames, attributeValues);
        }


        private ProductVariationsConfig.VariationGroup GetGroup(ProductVariationsConfig config, Product parentProduct, Variations variations)
        {
            ProductVariationsConfig.VariationGroup result = null;

            if (variations != null && variations.Product.Length > 1)
            {
                result = config.Groups.FirstOrDefault(g => IsMatch(g, variations));
                if (result == null)
                {
                    _logger.Warn("A variation group could not be found for a product with multiple variations productId: {0}"
                        , parentProduct.Id);
                }
            }

            return result;
        }

        private static Product GetProductByAttribute(IEnumerable<Product> products, string attributeName, string attributeValue)
        {
            return products.FirstOrDefault(p => string.Equals(p.CustomAttributes.ValueByName<string>(attributeName, null),
                                             attributeValue, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool IsMatch(ProductVariationsConfig.VariationGroup group, Variations variations)
        {
            // Here's where we need to make sure that each attribute has at least two distinct values
            // return group.Variations.All(v => !string.IsNullOrEmpty(variations.Products[0].CustomAttributes.ValueByName<string>(v.AttributeName, null)));
            if (variations == null || variations.Product.Length <= 1)
                return false;

            return @group.Variations.All(v =>
                                        !variations.Product.Any(p => String.IsNullOrEmpty(p.CustomAttributes.ValueByName<string>(v.AttributeName, null))) &&
                                        variations.Product
                                            .Select(p => p.CustomAttributes.ValueByName<string>(v.AttributeName, null))
                                            .Distinct()
                                            .Count() >= 2);
        }

        private static VariationsCollection.Option CreateOption(List<Product> products, Variations variations, ProductVariationsConfig.Variation groupVariation, string value)
        {
            var optionValue = value;
            var optionText = value;

            var alias = groupVariation.Aliases == null ? null : groupVariation.Aliases.FirstOrDefault(a => String.Equals(a.Value, optionValue, StringComparison.InvariantCultureIgnoreCase));
            if (alias != null)
            {
                var product = products.FirstOrDefault(
                    p => !String.IsNullOrEmpty(p.CustomAttributes.ValueByName<string>(groupVariation.AttributeName, null)));
                if (product != null)
                    optionText = product.CustomAttributes.ValueByName(alias.AttributeName, optionText);
            }

            var mask = groupVariation.Masks == null ? null : groupVariation.Masks.FirstOrDefault(a => String.Equals(a.Value, optionValue, StringComparison.InvariantCultureIgnoreCase))
                       ?? groupVariation.Masks.FirstOrDefault(a => a.Value == null);

            if (mask != null)
            {
                var var1 = mask.FormatVar0 != null ? String.Format(mask.FormatVar0, optionText) : optionText;

                string var2 = null;
                if (variations.Product.ToList().Select(p => p.Pricing.ListPrice.Value).Distinct().Count() > 1)
                {
                    var product = products.FirstOrDefault(
                        p => String.Equals(p.CustomAttributes.ValueByName<string>(groupVariation.AttributeName, null), value));
                    if (product != null)
                        var2 = mask.FormatVar1 != null
                                   ? String.Format(mask.FormatVar1, product.Pricing.ListPrice.Currency + " " + product.Pricing.ListPrice.Value)
                                   : product.Pricing.FormattedListPrice;
                }

                optionText = String.Format(mask.Format, var1, var2);
            }

            var result = new VariationsCollection.Option
            {
                Value = optionValue,
                Text = optionText
            };

            return result;
        }

        public async Task<BundleProductPickerViewModel> GetBundleProductPickerViewModel(long productId, string offerId)
        {
            try
            {
                return null;
                //var customBundleOffer = _catalogApi.GetCustomBundleOffer(productId, offerId);
                //if (customBundleOffer == null)
                //    return null;

                //var originalBundleGroups = new List<OfferBundleGroupViewModel>();
                //var fullyMandatoryBundleGroups = new List<OfferBundleGroupViewModel>();
                //var partiallyMandatoryBundleGroups = new List<OfferBundleGroupViewModel>();
                //var optionalBundleGroups = new List<OfferBundleGroupViewModel>();
                //foreach (var bundleGroup in customBundleOffer.OfferBundleGroups)
                //{
                //    if (bundleGroup.Mandatory)
                //    {
                //        if (bundleGroup.MaxProductQuantity == 1 && bundleGroup.MinProductQuantity == 1 &&
                //            bundleGroup.Products.Length == 1 && bundleGroup.Products[0].Product.ProductId == productId)
                //        {
                //            originalBundleGroups.Add(MapOfferBundleGroupToOfferBundleGroupViewModel(bundleGroup));
                //        }
                //        else if (bundleGroup.MaxProductQuantity == bundleGroup.MinProductQuantity &&
                //                 bundleGroup.MaxProductQuantity == bundleGroup.Products.Length)
                //            fullyMandatoryBundleGroups.Add(MapOfferBundleGroupToOfferBundleGroupViewModel(bundleGroup));
                //        else
                //            partiallyMandatoryBundleGroups.Add(MapOfferBundleGroupToOfferBundleGroupViewModel(bundleGroup));
                //    }
                //    else
                //        optionalBundleGroups.Add(MapOfferBundleGroupToOfferBundleGroupViewModel(bundleGroup));
                //}
                //// make sure each group is sorted by DisplayOrder
                //var comparer = new BundleGroupDisplayOrderComparer();
                //if (originalBundleGroups.Count > 1)
                //    originalBundleGroups.Sort(comparer);
                //if (fullyMandatoryBundleGroups.Count > 1)
                //    fullyMandatoryBundleGroups.Sort(comparer);
                //if (partiallyMandatoryBundleGroups.Count > 1)
                //    partiallyMandatoryBundleGroups.Sort(comparer);
                //if (optionalBundleGroups.Count > 1)
                //    optionalBundleGroups.Sort(comparer);
                //return new BundleProductPickerViewModel
                //{
                //    OfferId = customBundleOffer.Id,
                //    OfferName = customBundleOffer.Name,
                //    OfferInstanceId = customBundleOffer.OfferInstanceId,
                //    OriginalBundleGroups = originalBundleGroups.ToArray(),
                //    FullyMandatoryBundleGroups = fullyMandatoryBundleGroups.ToArray(),
                //    PartiallyMandatoryBundleGroups = partiallyMandatoryBundleGroups.ToArray(),
                //    OptionalBundleGroups = optionalBundleGroups.ToArray(),
                //};
            }
            catch (Exception)
            {
                return null;
            }
        }

        private OfferBundleGroupViewModel MapOfferBundleGroupToOfferBundleGroupViewModel(/*OfferBundleGroup group*/)
        {
            return null;
            //return new OfferBundleGroupViewModel
            //{
            //    BundleGroupInstanceId = group.BundleGroupInstanceId,
            //    BundleGroupName = group.BundleGroupName,
            //    MinProductQuantity = group.MinProductQuantity,
            //    MaxProductQuantity = group.MaxProductQuantity,
            //    Products = group.Products.Select(p => new BundleOfferProductViewModel
            //    {
            //        Product = p.Product,
            //        Pricing = p.Pricing,
            //        ProductDetailLink = p.ProductDetailLink,
            //        ProductSalesPitch = p.ProductSalesPitch,
            //        ProductOfferImage = p.ProductOfferImage,
            //        DisplayOrder = p.DisplayOrder,
            //        AvailableQuantity = p.AvailableQuantity,
            //        DetailMedia = GetProductMedia(p.Product.ProductId, p.Product.ProductId).ToArray()
            //    }).ToArray(),
            //    DisplayName = group.DisplayName,
            //    Description = group.Description,
            //    DisplayOrder = group.DisplayOrder
            //};
        }

        public Task<OffersViewModel> GetPromotion(string promotionName, Dictionary<string, string> attributes)
        {
            return null;
            //var offers = new OffersViewModel();
            //if (String.IsNullOrEmpty(promotionName))
            //{
            //    return offers;
            //}

            //var popResult = _catalogApi.GetPromotionWithProducts(promotionName);
            //if (popResult != null && popResult.Count > 0)
            //{
            //    foreach (var offer in popResult.OfferResults)
            //    {
            //        var ovm = new OfferViewModel
            //        {
            //            OfferId = offer.Id,
            //            DisplayName = offer.Attributes.StringByNameAfter("salesPitchKey1", "##") ?? ""
            //        };
            //        if (offer.OfferImages.Length > 0)
            //            ovm.ThumbnailImage = offer.OfferImages[0];

            //        // TODO Remove this hard-coded MS implementation to get the category ID
            //        ovm.CategoryId = ExtractCategoryIdFromImageLink(offer.Attributes.StringByName("offerImageLink") ?? ""); // hack for msstore
            //        ovm.Products = offer.Products == null
            //                   ? new List<SearchProduct>()
            //                   : offer.Products.Cast<SearchProduct>().ToList();

            //        if (offer.OfferImages.Length > 0)
            //            ovm.Image = offer.OfferImages.First();

            //        ovm.AltText = offer.Attributes.StringByName("offerImageAltText") ?? ovm.DisplayName;
            //        ovm.TargetUrl = offer.Attributes.StringByName("offerImageLink") ?? String.Empty;

            //        // mapping api payload attributes to view model
            //        var type = ovm.GetType();
            //        foreach (var key in attributes.Keys)
            //        {
            //            var value = offer.Attributes.StringByNameAfter(key, "##"); // hack to split GC values
            //            if (value != null)
            //            {
            //                var prop = type.GetProperty(attributes[key]);
            //                if (prop != null)
            //                {
            //                    prop.SetValue(ovm, value, null);
            //                }
            //            }
            //        }

            //        offers.Items.Add(ovm);
            //    }
            //}

            //return offers;
        }

        // TODO Move this logic to the web site project, as this is Microsoft Specific.
        // ugly hack to get the category from an offer
        // "/store/msstore/cat/categoryID.37286600?Icid=Home_4up_1_OfficeCatPage"

        private static string ExtractCategoryIdFromImageLink(string link)
        {
            var s = ExtractUrlId(link, "categoryID");
            if (String.IsNullOrEmpty(s))
            {
                // /store/msstore/html/pbpage.Windows_8_Pro?Icid=Home_4up_Win8
                s = ExtractUrlId(link, "pbpage");
            }
            return s;
        }

        /// <summary>
        /// Extract ID from GC style Urls, e.g.
        ///  /store/msstore/html/pbpage.Windows_8_Pro?Icid=Home_4up_Win8
        ///  /store/msstore/cat/categoryID.37286600?Icid=Home_4up_1_OfficeCatPage
        /// </summary>
        /// <param name="value">url input</param>
        /// <param name="name">name/indentifier to be extracted, e.g. "categoryID"</param>
        /// <returns>extracted id</returns>
        public static string ExtractUrlId(string value, string name)
        {
            var p = value.IndexOf(name, StringComparison.InvariantCultureIgnoreCase);
            if (p > 0)
            {
                var s = value.Substring(p + name.Length + 1); // add 1 for . or =
                var e = s.IndexOfAny(new[] { '?', '&' }); // terminator
                return e > 0 ? s.Substring(0, e) : s;
            }
            return null;
        }

        private void SetPDPImageProperties(string pdpPropertyName, ProductMediaViewModel[] productMediaViewModels/*, IEnumerable<ExtendedAttribute> attributes*/,
                                        Action<ProductMediaViewModel, string> setter)
        {
            /*
            foreach (var attribute in attributes)
            {
                var i = Int32.Parse(attribute.Name.Substring(pdpPropertyName.Length)) - 1;

                if (i < 0 || i >= productMediaViewModels.Length) continue;

                var detialMediaItem = productMediaViewModels[i];

                if (detialMediaItem == null)
                {
                    productMediaViewModels[i] = (detialMediaItem = new ProductMediaViewModel());
                }

                setter(detialMediaItem, attribute.Value);
            }*/
        }

        private class BundleGroupDisplayOrderComparer : IComparer<OfferBundleGroupViewModel>
        {
            public int Compare(OfferBundleGroupViewModel x, OfferBundleGroupViewModel y)
            {
                return x.DisplayOrder.CompareTo(y.DisplayOrder);
            }
        }
    }

    public static class ProductBuilderExtensions
    {
        public static VariationsCollection.Variation FirstOrDefault(this VariationsCollection collection, string levelId)
        {
            return FirstOrDefault(collection.Variations, levelId);
        }

        public static VariationsCollection.Variation FirstOrDefault(this VariationsCollection.Variation variations, string levelId)
        {
            if (variations != null)
            {
                if (string.Equals(variations.Id, levelId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return variations;
                }

                if (variations.Options.Length > 0)
                {
                    return FirstOrDefault(variations.Options[0].Variations, levelId);
                }
            }

            return null;
        }
    }
}
