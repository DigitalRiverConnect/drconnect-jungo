using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Utils;
using Jungo.Infrastructure.Config.Models;
using Jungo.Infrastructure.Extensions;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class ProductDetailPageViewModel : PageViewModelBase
    {
        private readonly Dictionary<string, object> _attributeCache = new Dictionary<string, object>();
        private const string ProductTypePhysical = "PHYSICAL";

        public ProductDetailPageViewModel(SiteInfo siteInfo)
        {
            _attributeCache = new Dictionary<string, object>();
            SiteInfo = siteInfo;
        }

        public Product Product { get; set; }
        public Product DisplayProduct { get; set; }
        public VariationsCollection VariationOptions { get; set; }
        public IEnumerable<VariationsCollection.Variation> VariationLevels { get; set; }
        public string VariationOptionsJson { get; set; }

        public SiteInfo SiteInfo { get; private set; }
        public BuyViewModel BuyViewModel { get; set; }

        private bool? _isPhysical;
        public bool IsPhysical {
            get
            {
                return _isPhysical ??
                (_isPhysical =
                    Product.ProductType.Equals(ProductTypePhysical, StringComparison.InvariantCultureIgnoreCase)).Value;
            } 
        }

        private long _parentProductId;
        public long ParentProductId
        {
            get
            {
                if (_parentProductId == 0) _parentProductId = Product.GetParentProductId();
                return _parentProductId;
            }
        }

        public bool IsPreOrderEnabled
        {
            get { return Product.InventoryStatus.ProductIsAllowsBackorders; }
        }

        public string Footer
        {
            get { return Product.CustomAttributes.ValueByName("ProductDetailsFooter", ""); }
        }

        public bool IsDownloadable
        {
            get { return Product.CustomAttributes.ValueByName("isDownloadable", false); }
        }

        // todo: when we have room in the sprint to address this, address it; it's ms-specific but should be generic
        public int MinimumAgeToView
        {
            get { return Product.CustomAttributes.ValueByName("ageGate", 0); }
        }

        public bool HasVariationOptions
        {
            get { return VariationOptions != null && VariationOptions.Variations.Options.Length > 0; }
        }

        public override string SeoMetaKeywords
        {
            get { return Product.CustomAttributes.ValueByName("Meta Keywords", ""); }
            set { }
        }

        public override string SeoMetaDescription
        {
            get { return Product.LongDescription; }
            set { }
        }

        public T GetAttribute<T>(string name) where T : IConvertible
        {
            object value;
            if (_attributeCache.TryGetValue(name, out value))
            {
                return (T)value;
            }

            var attr = FindAttributeByName(name);
            if (attr == null || string.IsNullOrEmpty(attr.Value)) return default(T); // avoid exception which slows down execution

            value = (T)Convert.ChangeType(attr.Value, typeof(T));
            _attributeCache.Add(name, value);
            return (T)value;
        }

        private Jungo.Models.ShopperApi.Common.Attribute FindAttributeByName(string name)
        {
            if (Product.CustomAttributes == null || Product.CustomAttributes.Attribute == null)
                return null;

            return
                Product.CustomAttributes.Attribute.FirstOrDefault(
                    attribute =>
                    attribute.Name != null &&
                    attribute.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
