using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models
{
    /// <summary>
    /// a way for pages to inform about themselves to whatever needs it (such as analytics or a live chat service)
    /// </summary>
    public interface IPageInfo
    {
        /// <summary>
        /// general name/value pair
        /// </summary>
        void AddProperty(string name, string value);

        // specific named values, for convenience instead of using AddProperty
        string PageName { get; set; } // using a dotted hierarchical naming convention
        decimal CartTotal { get; set; }
        int ConversionStage { get; set; }
        int ErrorCount { get; set; }
        int OrderLineItemCount { get; set; }
        int ProductTotalCount { get; set; }
        string ProductStatus { get; set; }
        string ProductName { get; set; }
        decimal ProductPrice { get; set; }
        decimal OrderTotal { get; set; }
        string OrderNumber { get; set; }
        string SearchWord { get; set; }
        string Category { get; set; }
        int SearchResults { get; set; }

        // when info is all set, call this to get a general property bag
        IDictionary<string, string> PropertyBag { get; }
    }

    public class PageInfo : IPageInfo
    {
        public const string PageInfoPropertyBagSlot = "PageInfoPropertyBag";

        public const string PropertyCartTotal = "cartTotal";
        public const string PropertyCategory = "category";
        public const string PropertyConversionStage = "conversionStage";
        public const string PropertyErrorCount = "errorCount";
        public const string PropertyOrderCount = "orderCount";
        public const string PropertyOrderNumber = "orderNumber";
        public const string PropertyOrderTotal = "orderTotal";
        public const string PropertyPageName = "pageName";
        public const string PropertyProductCount = "productCount"; // same number as OrderCount
        public const string PropertyProductName = "productName";
        public const string PropertyProductPrice = "productPrice";
        public const string PropertyProductStatus = "productStatus";
        public const string PropertyProductTotalCount = "productTotal";
        public const string PropertySearchResults = "searchResults";
        public const string PropertySearchWord = "searchWord";

        #region IPageInfo Members

        public void AddProperty(string name, string value)
        {
            var propbag = PropertyBag;
            if (propbag.ContainsKey(name))
                propbag.Remove(name);
            if (!string.IsNullOrEmpty(value))
                propbag.Add(name, value);
        }

        public string GetProperty(string name)
        {
            var propbag = (IDictionary<string, string>)HttpContext.Current.Items[PageInfoPropertyBagSlot];
            if (propbag == null)
                return string.Empty;
            return propbag.ContainsKey(name) ? propbag[name] : string.Empty;
        }

        public string PageName
        {
            get { return GetProperty(PropertyPageName); }
            set { AddProperty(PropertyPageName, value); }
        }

        public decimal CartTotal
        {
            get { return GetDecimalProperty(PropertyCartTotal); }
            set { AddProperty(PropertyCartTotal, value); }
        }

        public int ConversionStage
        {
            get { return GetIntProperty(PropertyConversionStage); }
            set { AddProperty(PropertyConversionStage, value); }
        }

        public int ErrorCount
        {
            get { return GetIntProperty(PropertyErrorCount); }
            set { AddProperty(PropertyErrorCount, value); }
        }

        public int OrderLineItemCount
        {
            get { return GetIntProperty(PropertyOrderCount); }
            set
            {
                AddProperty(PropertyOrderCount, value);
                AddProperty(PropertyProductCount, value);
            }
        }

        public int ProductTotalCount
        {
            get { return GetIntProperty(PropertyProductTotalCount); }
            set { AddProperty(PropertyProductTotalCount, value); }
        }

        public string ProductStatus
        {
            get { return GetProperty(PropertyProductStatus); }
            set { AddProperty(PropertyProductStatus, value); }
        }

        public string ProductName
        {
            get { return GetProperty(PropertyProductName); }
            set { AddProperty(PropertyProductName, value); }
        }

        public decimal ProductPrice
        {
            get { return GetDecimalProperty(PropertyProductPrice); }
            set { AddProperty(PropertyProductPrice, value); }
        }

        public decimal OrderTotal
        {
            get { return GetDecimalProperty(PropertyOrderTotal); }
            set { AddProperty(PropertyOrderTotal, value); }
        }

        public string OrderNumber
        {
            get { return GetProperty(PropertyOrderNumber); }
            set { AddProperty(PropertyOrderNumber, value); }
        }

        public string SearchWord
        {
            get { return GetProperty(PropertySearchWord); }
            set { AddProperty(PropertySearchWord, value); }
        }

        public string Category
        {
            get { return GetProperty(PropertyCategory); }
            set { AddProperty(PropertyCategory, value); }
        }

        public int SearchResults
        {
            get { return GetIntProperty(PropertySearchResults); }
            set { AddProperty(PropertySearchResults, value); }
        }

        public IDictionary<string, string> PropertyBag
        {
            get
            {
                var propbag = (IDictionary<string, string>)HttpContext.Current.Items[PageInfoPropertyBagSlot];
                if (propbag == null)
                {
                    propbag = new Dictionary<string, string>();
                    HttpContext.Current.Items.Add(PageInfoPropertyBagSlot, propbag);
                }
                return propbag;
            }
        }

        #endregion

        #region internals

        private void AddProperty(string name, int value)
        {
            AddProperty(name, value.ToString(CultureInfo.InvariantCulture));
        }

        private void AddProperty(string name, decimal value)
        {
            AddProperty(name, value.ToString("0.00"));
        }

        private decimal GetDecimalProperty(string name)
        {
            var t = GetProperty(name);
            return string.IsNullOrEmpty(t) ? 0.00m : decimal.Parse(GetProperty(name));
        }

        private int GetIntProperty(string name)
        {
            var t = GetProperty(name);
            return string.IsNullOrEmpty(t) ? 0 : int.Parse(GetProperty(name));
        }

        #endregion
    }
}
