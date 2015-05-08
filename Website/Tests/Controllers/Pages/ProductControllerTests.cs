//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Common;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using Moq;
//using N2.Web;
//using N2.Web.Mvc;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Pages
//{
//    public class ProductControllerTests : TestBase
//    {
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();

//        public ProductControllerTests()
//        {
//            DependencyRegistrar.
//                StandardDependencies()
//                .With(_catalogAdapter.Object);
//        }

//        [Fact]
//        public void GetPageTitleFromModel_NullModel()
//        {
//            var res = ProductController.GetPageTitleFromModel(null, null);

//            Assert.True(string.IsNullOrEmpty(res));
//        }

//        [Fact]
//        public void GetPageTitleFromModel_1stFromProductAttribute()
//        {
//            var mod = new ProductDetailPageViewModel()
//            {
//                Product =
//                    new Product
//                    {
//                        Attributes = new[] { new ExtendedAttribute { Name = "Custom Title", Value = "custom title" } }
//                    }
//            };

//            var res = ProductController.GetPageTitleFromModel(mod, null);

//            Assert.Contains("custom title", res);
//        }

//        [Fact]
//        public void GetPageTitleFromModel_2ndFromParentProductAttribute()
//        {
//            var mod = new ProductDetailPageViewModel()
//            {
//                ParentProductId = "parentid",
//                Product = new Product { Attributes = new ExtendedAttribute[0], Id = "productid" }
//            };
//            var parentProd = new Product
//            {
//                Attributes = new[] {new ExtendedAttribute {Name = "Custom Title", Value = "parent custom title"}}
//            };
//            _catalogAdapter.Setup(c => c.TryGetProduct("parentid", out parentProd))
//                .Returns(true);

//            var res = ProductController.GetPageTitleFromModel(mod, _catalogAdapter.Object);

//            Assert.Contains("parent custom title", res);
//        }

//        [Fact]
//        public void GetPageTitleFromModel_3rdFromResource()
//        {
//            var mod = new ProductDetailPageViewModel()
//            {
//                ParentProductId = "productid",
//                Product = new Product { Attributes = new ExtendedAttribute[0], Id = "productid" }
//            };
//            FakeResources.AddString("PageTitle", "Product", "resource title");

//            var res = ProductController.GetPageTitleFromModel(mod, null);

//            Assert.Contains("resource title", res);
//        }

//        [Fact]
//        public void GetPageTitleFromModel_3rdFromResourceWithSubstitution()
//        {
//            var mod = new ProductDetailPageViewModel()
//            {
//                ParentProductId = "productid",
//                Product = new Product { Attributes = new ExtendedAttribute[0], Id = "productid", Title="product name" }
//            };
//            FakeResources.AddString("PageTitle", "Product", "resource title : {productname}");

//            var res = ProductController.GetPageTitleFromModel(mod, null);

//            Assert.Contains("resource title : product name", res);
//        }

//        [Fact]
//        public void GetPageTitle_1stFromCurrentItem()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyProductController>();
//            ctrl.MyCurrentPage.PageTitle = "item page title";

//            var res = ctrl.CallGetPageTitle(null);

//            Assert.Contains("item page title", res);
//        }

//        [Fact]
//        public void GetPageTitle_2ndFromModel()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyProductController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;

//            var res = ctrl.CallGetPageTitle(new ProductDetailPageViewModel()
//            {
//                Product =
//                    new Product
//                    {
//                        Attributes = new[] {new ExtendedAttribute {Name = "Custom Title", Value = "custom title"}}
//                    }
//            });

//            Assert.Contains("custom title", res);
//        }

//        [Fact]
//        public void GetPageTitle_3rdFromResource()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyProductController>();
//            ctrl.MyCurrentPage.PageTitle = string.Empty;
//            FakeResources.AddString("PageTitle", "PAGETYPE_Product", "resource title");

//            var res = ctrl.CallGetPageTitle(null);

//            Assert.Contains("resource title", res);
//        }

//        private class MyProductController : ProductController
//        {
//            public MyProductController(ICatalogAdapter catalogAdapter)
//                : base(null, catalogAdapter, null, null, null, null)
//            {
//                Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage));
//            }

//            public ProductPage MyCurrentPage = new ProductPage();

//            // promote for calling in tests
//            public string CallGetPageTitle(ProductDetailPageViewModel model)
//            {
//                return GetPageTitle(model);
//            }
//        }
//    }
//}
