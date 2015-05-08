using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models;
using Moq;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Web
{
    public class PageInfoTests : TestBase
    {
        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
        private readonly IDictionary<string, string> _propBag = new Dictionary<string, string>();
       
        public PageInfoTests()
        {
            _webSession.Setup(w => w.Get<IDictionary<string, string>>(PageInfo.PageInfoPropertyBagSlot))
                       .Returns(_propBag);
            WebSession.Current = _webSession.Object;
            HttpContext.Current = new HttpContext(new HttpRequest("test", "http://localhost", null), new HttpResponse(new StreamWriter(new MemoryStream())));

            // Default to en-us unless specified explicitly, avoid exception on "auto"
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }

        #region specific property members
        // test: SearchWord
        [Fact]
        public void SearchWord()
        {
            var target = new PageInfo();
            const string expected = "junk";
            target.SearchWord = expected;
            var actual = target.SearchWord;
            Assert.Equal(expected, actual);
        }

        // test: SearchResults
        [Fact]
        public void SearchResults()
        {
            var target = new PageInfo();
            const int expected = 55;
            target.SearchResults = expected;
            var actual = target.SearchResults;
            Assert.Equal(expected, actual);
        }

        // test: ProductTotalCount
        [Fact]
        public void ProductTotalCount()
        {
            var target = new PageInfo();
            const int expected = 66;
            target.ProductTotalCount = expected;
            var actual = target.ProductTotalCount;
            Assert.Equal(expected, actual);
        }

        // test: ProductStatus
        [Fact]
        public void ProductStatus()
        {
            var target = new PageInfo();
            const string expected = "garbage";
            target.ProductStatus = expected;
            var actual = target.ProductStatus;
            Assert.Equal(expected, actual);
        }

        // test: ProductPrice
        [Fact]
        public void ProductPrice()
        {
            var target = new PageInfo();
            const decimal expected = 77.77m;
            target.ProductPrice = expected;
            var actual = target.ProductPrice;
            Assert.Equal(expected, actual);
        }

        // test: ProductName
        [Fact]
        public void ProductName()
        {
            var target = new PageInfo();
            const string expected = "detritus";
            target.ProductName = expected;
            var actual = target.ProductName;
            Assert.Equal(expected, actual);
        }

        // test: OrderTotal
        [Fact]
        public void OrderTotal()
        {
            var target = new PageInfo();
            const decimal expected = 88.88m;
            target.OrderTotal = expected;
            var actual = target.OrderTotal;
            Assert.Equal(expected, actual);
        }

        // test: OrderNumber
        [Fact]
        public void OrderNumber()
        {
            var target = new PageInfo();
            const string expected = "refuse";
            target.OrderNumber = expected;
            var actual = target.OrderNumber;
            Assert.Equal(expected, actual);
        }

        // test: OrderLineItemCount
        [Fact]
        public void OrderLineItemCount()
        {
            var target = new PageInfo();
            const int expected = 4;
            target.OrderLineItemCount = expected;
            var actual = target.OrderLineItemCount;
            Assert.Equal(expected, actual);
            // OrderLineItemCount generates another property:
            Assert.Equal(expected.ToString(), target.GetProperty(PageInfo.PropertyProductCount));
        }

        // test: ErrorCount
        [Fact]
        public void ErrorCount()
        {
            var target = new PageInfo();
            const int expected = 3;
            target.ErrorCount = expected;
            var actual = target.ErrorCount;
            Assert.Equal(expected, actual);
        }

        // test: ConversionStage
        [Fact]
        public void ConversionStage()
        {
            var target = new PageInfo();
            const int expected = 2;
            target.ConversionStage = expected;
            var actual = target.ConversionStage;
            Assert.Equal(expected, actual);
        }

        // test: Category
        [Fact]
        public void Category()
        {
            var target = new PageInfo();
            const string expected = "waste";
            target.Category = expected;
            var actual = target.Category;
            Assert.Equal(expected, actual);
        }

        // test: CartTotal
        [Fact]
        public void CartTotal()
        {
            var target = new PageInfo();
            const decimal expected = 11.11m;
            target.CartTotal = expected;
            var actual = target.CartTotal;
            Assert.Equal(expected, actual);
        }
        #endregion

        #region other methods
        // test: GetProperty on non-existing property returns empty string, i.e., doesn't blow up
        [Fact]
        public void GetProperty_NonExists()
        {
            var target = new PageInfo();
            Assert.Equal(string.Empty, target.GetProperty("oops"));
        }

        // test: PropertyBag is empty to begin with
        [Fact]
        public void PropertyBag_Empty()
        {
            var target = new PageInfo();
            Assert.Equal(0, target.PropertyBag.Count);
        }

        // test: PropertyBag has values set via properties
        [Fact]
        public void PropertyBag_Values()
        {
            var target = new PageInfo
                {
                    CartTotal = 1.23m,
                    Category = "cat",
                    ConversionStage = 4,
                    ErrorCount = 5,
                    OrderLineItemCount = 6, // generates 2 identical properties
                    OrderNumber = "ord#",
                    OrderTotal = 7.89m,
                    ProductName = "prodname",
                    ProductPrice = 2.34m,
                    ProductStatus = "prodstat",
                    ProductTotalCount = 5,
                    SearchResults = 6,
                    SearchWord = "srchwrd"
                };
            Assert.Equal(14, target.PropertyBag.Count);
            Assert.Equal("1.23", target.PropertyBag[PageInfo.PropertyCartTotal]);
            Assert.Equal("cat", target.PropertyBag[PageInfo.PropertyCategory]);
            Assert.Equal("4", target.PropertyBag[PageInfo.PropertyConversionStage]);
            Assert.Equal("5", target.PropertyBag[PageInfo.PropertyErrorCount]);
            Assert.Equal("6", target.PropertyBag[PageInfo.PropertyOrderCount]);
            Assert.Equal("6", target.PropertyBag[PageInfo.PropertyProductCount]);
            Assert.Equal("ord#", target.PropertyBag[PageInfo.PropertyOrderNumber]);
            Assert.Equal("7.89", target.PropertyBag[PageInfo.PropertyOrderTotal]);
            Assert.Equal("prodname", target.PropertyBag[PageInfo.PropertyProductName]);
            Assert.Equal("2.34", target.PropertyBag[PageInfo.PropertyProductPrice]);
            Assert.Equal("prodstat", target.PropertyBag[PageInfo.PropertyProductStatus]);
            Assert.Equal("5", target.PropertyBag[PageInfo.PropertyProductTotalCount]);
            Assert.Equal("6", target.PropertyBag[PageInfo.PropertySearchResults]);
            Assert.Equal("srchwrd", target.PropertyBag[PageInfo.PropertySearchWord]);
        }

        // test: PropertyBag has values set via AddProperty
        [Fact]
        public void PropertyBag_CustomValues()
        {
            var target = new PageInfo();
            const string propName = "myProp";
            const string propValue = "myPropValue";
            target.AddProperty(propName, propValue);
            Assert.Equal(1, target.PropertyBag.Count);
            Assert.Equal(propValue, target.PropertyBag[propName]);
            Assert.Equal(propValue, target.GetProperty(propName));
        }

        // test: AddProperty allows 2nd call to set same property
        [Fact]
        public void PropertyBag_AddPropertyTwice()
        {
            var target = new PageInfo();
            const string propName = "myProp";
            const string propValue1 = "myPropValue1";
            const string propValue2 = "myPropValue2";
            target.AddProperty(propName, propValue1);
            Assert.Equal(1, target.PropertyBag.Count);
            Assert.Equal(propValue1, target.PropertyBag[propName]);
            target.AddProperty(propName, propValue2);
            Assert.Equal(1, target.PropertyBag.Count); // still only one property
            Assert.Equal(propValue2, target.PropertyBag[propName]); // but with different value now
        }
        #endregion
    }
}
