//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Web.Mvc;
//using DigitalRiver.CloudLink.Commerce.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Common;
//using DigitalRiver.CloudLink.Commerce.Api.Session;
//using DigitalRiver.CloudLink.Commerce.Api.ShoppingCart;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart;
//using DigitalRiver.CloudLink.Commerce.Web.Resources;
//using DigitalRiver.CloudLink.Core.Services.Cache;
//using DigitalRiver.CloudLink.Core.Services.Logging;
//using Moq;
//using ViewModelBuilders.Cart;
//using ViewModelBuilders.Catalog;
//using Xunit;
//using ShoppingCartViewModel = DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart.ShoppingCartViewModel;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Parts
//{
//    public class ShoppingCartPartControllerTests : TestBase
//    {
//        private readonly Mock<IShoppingCartAdapter> _shoppingCartAdapter = new Mock<IShoppingCartAdapter>();
//        private readonly Mock<IShoppingCartViewModelBuilder> _shoppingCartViewModelBuilder = new Mock<IShoppingCartViewModelBuilder>();
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
//        private readonly Mock<ISessionAdapter> _sessionAdapter = new Mock<ISessionAdapter>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<IResourceService> _resourceService = new Mock<IResourceService>();
//        private readonly Mock<ISessionInfoResolver> _sessionInfoResolver = new Mock<ISessionInfoResolver>();
//        private readonly HttpContextTestData _httpContextTestData = new HttpContextTestData();

//        public ShoppingCartPartControllerTests()
//        {
//            DependencyRegistrar
//                .StandardDependencies()
//                .WithFakeLogger()
//                .With(_shoppingCartAdapter.Object)
//                .With(_shoppingCartViewModelBuilder.Object)
//                .With(_catalogAdapter.Object)
//                .With(_sessionAdapter.Object)
//                .With(_linkGenerator.Object)
//                .With(_sessionInfoResolver.Object)
//                .With(_resourceService.Object)
//                .With(_httpContextTestData)
//                .WithSingleton<ICacheService, InMemoryCacheService>()
//                .WithSingleton<IProductViewModelBuilder, MyProductViewModelBuilder>();
//            WebSession.Current = _webSession.Object;
//        }

//        #region Index
//        // test: Index() returns PartialViewResult
//        [Fact]
//        public void Index_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            _shoppingCartAdapter.Setup(c => c.GetShippingMethods()).Returns(new ShippingMethod[0]);
//            _shoppingCartAdapter.Setup(c => c.GetCheckoutInfo()).Returns(new CheckoutInfo());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            ctrl.SetMockControllerContext(_httpContextTestData);

//            // test
//            var res = ctrl.Index();

//            // sense
//            Assert.True(res is PartialViewResult);
//        }

//        // test: Index() renders Index
//        [Fact]
//        public void Index_Render()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            _shoppingCartAdapter.Setup(c => c.GetShippingMethods()).Returns(new ShippingMethod[0]);
//            _shoppingCartAdapter.Setup(c => c.GetCheckoutInfo()).Returns(new CheckoutInfo());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            ctrl.SetMockControllerContext(_httpContextTestData);

//            // test
//            var res = ctrl.Index();

//            // sense
//            var vres = res as PartialViewResult; // a different test assures we have a PartialViewResult; no need to retest this here
//            Assert.Equal("Index", vres.ViewName);
//        }

//        // test: Index returns a model of type ShoppingCartViewModel
//        [Fact]
//        public void Index_ModelType()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            _shoppingCartAdapter.Setup(c => c.GetShippingMethods()).Returns(new ShippingMethod[0]);
//            _shoppingCartAdapter.Setup(c => c.GetCheckoutInfo()).Returns(new CheckoutInfo());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            ctrl.SetMockControllerContext(_httpContextTestData);

//            // test
//            var vres = ctrl.Index() as PartialViewResult; // a different test assures we have a PartialViewResult; no need to retest this here

//            // sense
//            Assert.True(vres.Model is ShoppingCartViewModel);
//        }
//        #endregion

//        #region GetCart
//        // test: GetCart() returns PartialViewResult
//        [Fact]
//        public void GetCart_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            _shoppingCartAdapter.Setup(c => c.GetShippingMethods()).Returns(new ShippingMethod[0]);
//            _shoppingCartAdapter.Setup(c => c.GetCheckoutInfo()).Returns(new CheckoutInfo());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.GetCart();

//            // sense
//            Assert.True(res is PartialViewResult);
//        }

//        // test: GetCart() renders ShoppingCart
//        [Fact]
//        public void GetCart_Render()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            _shoppingCartAdapter.Setup(c => c.GetShippingMethods()).Returns(new ShippingMethod[0]);
//            _shoppingCartAdapter.Setup(c => c.GetCheckoutInfo()).Returns(new CheckoutInfo());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.GetCart();

//            // sense
//            var vres = res as PartialViewResult; // a different test assures we have a PartialViewResult; no need to retest this here
//            Assert.Equal("ShoppingCart", vres.ViewName);
//        }

//        // test: GetCart returns a model of type ShoppingCartViewModel
//        [Fact]
//        public void GetCart_ModelType()
//        {
//            // setup
//            _shoppingCartViewModelBuilder.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCartViewModel());
//            _shoppingCartAdapter.Setup(c => c.GetShippingMethods()).Returns(new ShippingMethod[0]);
//            _shoppingCartAdapter.Setup(c => c.GetCheckoutInfo()).Returns(new CheckoutInfo());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.GetCart() as PartialViewResult; // a different test assures we have a PartialViewResult; no need to retest this here

//            // sense
//            Assert.True(vres.Model is ShoppingCartViewModel);
//        }
//        #endregion

//        #region AddProduct

//        // test: AddProduct calls shopping cart service AddProduct with appropriate params
//        [Fact]
//        public void AddProduct_Adds()
//        {
//            // setup
//            var prodId = string.Empty; // sensing variables
//            var qty = string.Empty; // "
//            var offerId = string.Empty; // "
//            var errors = new List<string>();
//            _shoppingCartAdapter.Setup(
//                c => c.AddProductWithErrors(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out errors))
//                .OutCallback((string pId, string q, string oId, out List<string> errList) =>
//                {
//                    prodId = pId;
//                    qty = q;
//                    offerId = oId;
//                    errList = new List<string>();
//                }).Returns(new ShoppingCart());
//            var prod = new AddProductModel {ProductId = "123", Quantity = "4", OfferId = "567"};
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.AddProduct(prod);

//            // sense
//            Assert.Equal(prod.ProductId, prodId);
//            Assert.Equal(prod.Quantity, qty);
//            Assert.Equal(prod.OfferId, offerId);
//        }

//        // test: AddProduct returns PartialViewResult
//        [Fact]
//        public void AddProduct_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.AddProduct(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ShoppingCart) null);
//            var prod = new AddProductModel {ProductId = "123", Quantity = "4", OfferId = "567"};
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.AddProduct(prod);

//            // sense
//            Assert.True(res is PartialViewResult, "should have returned PartialViewResult");
//        }

//        // test: AddProduct returns SimpleResponseViewModel
//        [Fact]
//        public void AddProduct_Model()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.AddProduct(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ShoppingCart)null);
//            var prod = new AddProductModel { ProductId = "123", Quantity = "4", OfferId = "567" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.AddProduct(prod) as PartialViewResult;

//            // sense
//            Assert.NotNull(vres.Model);
//            Assert.True(vres.Model is SimpleResponseViewModel, "should have returned SimpleResponseViewModel");
//        }

//        // test: AddProduct handles Exception
//        [Fact]
//        public void AddProduct_Exception()
//        {
//            // setup
//            var errors = new List<string> {"AddProduct_Failed Message"};
//            _shoppingCartAdapter.Setup(c => c.AddProductWithErrors(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out errors))
//              .Returns((ShoppingCart)null);
//            var prod = new AddProductModel {ProductId = "123", Quantity = "4", OfferId = "567"};
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.AddProduct(prod) as PartialViewResult;
//                // a different test assures we have a PartialViewResult; no need to retest this here
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("AddProduct_Failed Message", mod.ErrorMessages.First());
//        }

//        #endregion

//        #region UpdateLineItem

//        // test: UpdateLineItem calls shopping cart service UpdateLineItem with appropriate params
//        [Fact]
//        public void UpdateLineItem_Updates()
//        {
//            // setup
//            var lineId = string.Empty; // sensing variables
//            var qty = string.Empty; // "
//            _shoppingCartAdapter.Setup(c => c.UpdateLineItem(It.IsAny<string>(), It.IsAny<string>()))
//              .Callback<string, string>((lId, q) =>
//              {
//                  lineId = lId;
//                  qty = q;
//              });
//            var upd = new UpdateLineItemModel { LineItemId = "123", Quantity = 4 };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.UpdateLineItem(upd);

//            // sense
//            Assert.Equal(upd.LineItemId, lineId);
//            Assert.Equal(upd.Quantity.ToString(CultureInfo.InvariantCulture), qty);
//        }

//        // test: UpdateLineItem returns PartialViewResult
//        [Fact]
//        public void UpdateLineItem_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateLineItem(It.IsAny<string>(), It.IsAny<string>()));
//            var upd = new UpdateLineItemModel { LineItemId = "123", Quantity = 4 };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.UpdateLineItem(upd);

//            // sense
//            Assert.True(res is PartialViewResult, "should have returned PartialViewResult");
//        }

//        // test: UpdateLineItem returns SimpleResponseViewModel
//        [Fact]
//        public void UpdateLineItem_Model()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateLineItem(It.IsAny<string>(), It.IsAny<string>()));
//            var upd = new UpdateLineItemModel { LineItemId = "123", Quantity = 4 };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.UpdateLineItem(upd) as PartialViewResult;

//            // sense
//            Assert.NotNull(vres.Model);
//            Assert.True(vres.Model is SimpleResponseViewModel, "should have returned SimpleResponseViewModel");
//        }

//        // test: UpdateLineItem returns success
//        [Fact]
//        public void UpdateLineItem_Success()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateLineItem(It.IsAny<string>(), It.IsAny<string>()));
//            var upd = new UpdateLineItemModel { LineItemId = "123", Quantity = 4 };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.UpdateLineItem(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.True(mod.Success);
//        }

//        // test: UpdateLineItem handles ShoppingCartLockedException
//        [Fact]
//        public void UpdateLineItem_ShoppingCartLockedException()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateLineItem(It.IsAny<string>(), It.IsAny<string>()))
//              .Throws(new ShoppingCartLockedException());
//            var upd = new UpdateLineItemModel { LineItemId = "123", Quantity = 4 };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartAdapter.ErrMsgAddprodCartLocked,
//                                            "CartLocked Message");

//            // test
//            var vres = ctrl.UpdateLineItem(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("CartLocked Message", mod.ErrorMessages.First());
//        }

//        // test: UpdateLineItem handles Exception
//        [Fact]
//        public void UpdateLineItem_Exception()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateLineItem(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
//            var upd = new UpdateLineItemModel { LineItemId = "123", Quantity = 4 };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgUpdateLineItemFailed,
//                                            "Exception Message");

//            // test
//            var vres = ctrl.UpdateLineItem(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("Exception Message", mod.ErrorMessages.First());
//        }
//        #endregion

//        #region AddCoupon

//        // test: AddCoupon calls shopping cart service AddCoupon with appropriate params
//        [Fact]
//        public void AddCoupon_Adds()
//        {
//            // setup
//            var coupon = string.Empty; // sensing variables
//            _shoppingCartAdapter.Setup(c => c.AddCoupon(It.IsAny<string>()))
//              .Callback<string>((cp) =>
//              {
//                  coupon = cp;
//              });
//            var coupMod = new AddCouponModel {CouponCode = "1234"};
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            ctrl.AddCoupon(coupMod);

//            // sense
//            Assert.Equal(coupMod.CouponCode, coupon);
//        }

//        // test: AddCoupon returns PartialViewResult
//        [Fact]
//        public void AddCoupon_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.AddCoupon(It.IsAny<string>())).Returns((ShoppingCart)null);
//            var coupMod = new AddCouponModel { CouponCode = "1234" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.AddCoupon(coupMod);

//            // sense
//            Assert.True(res is PartialViewResult, "should have returned PartialViewResult");
//        }

//        // test: AddCoupon handles ShoppingCartLockedException
//        [Fact]
//        public void AddCoupon_ShoppingCartLockedException()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.AddCoupon(It.IsAny<string>())).Throws(new ShoppingCartLockedException());
//            var coupMod = new AddCouponModel { CouponCode = "1234" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartAdapter.ErrMsgAddprodCartLocked,
//                                            "CartLocked Message");

//            // test
//            var vres = ctrl.AddCoupon(coupMod) as PartialViewResult;
//            // a different test assures we have a PartialViewResult; no need to retest this here
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("CartLocked Message", mod.ErrorMessages.First());
//        }

//        // test: AddCoupon handles Exception
//        [Fact]
//        public void AddCoupon_Exception()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.AddCoupon(It.IsAny<string>())).Throws(new Exception());
//            var coupMod = new AddCouponModel { CouponCode = "1234" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgAddCouponFailed,
//                                            "AddCoupon_Failed Message");

//            // test
//            var vres = ctrl.AddCoupon(coupMod) as PartialViewResult;
//            // a different test assures we have a PartialViewResult; no need to retest this here
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("AddCoupon_Failed Message", mod.ErrorMessages.First());
//        }
//        #endregion

//        #region ChangeVariationFromLineItemToProduct

//        // test: ChangeVariationFromLineItemToProduct calls shopping cart service ChangeVariationFromLineItemIdToProductId with appropriate params
//        [Fact]
//        public void ChangeVariation_Updates()
//        {
//            // setup
//            var lineId = string.Empty; // sensing variables
//            var prodId = string.Empty; // "
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()))
//              .Callback<string, string>((lId, pId) =>
//              {
//                  lineId = lId;
//                  prodId = pId;
//              });
//            var chg = new ChangeVariationModel {LineItemId = "123", ProductId = "456"};
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.ChangeVariationFromLineItemToProduct(chg);

//            // sense
//            Assert.Equal(chg.LineItemId, lineId);
//            Assert.Equal(chg.ProductId.ToString(CultureInfo.InvariantCulture), prodId);
//        }

//        // test: ChangeVariationFromLineItemToProduct returns PartialViewResult
//        [Fact]
//        public void ChangeVariation_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()));
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.ChangeVariationFromLineItemToProduct(chg);

//            // sense
//            Assert.True(res is PartialViewResult, "should have returned PartialViewResult");
//        }

//        // test: ChangeVariationFromLineItemToProduct returns SimpleResponseViewModel
//        [Fact]
//        public void ChangeVariation_Model()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()));
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;

//            // sense
//            Assert.NotNull(vres.Model);
//            Assert.True(vres.Model is SimpleResponseViewModel, "should have returned SimpleResponseViewModel");
//        }

//        // test: ChangeVariationFromLineItemToProduct returns success
//        [Fact]
//        public void ChangeVariation_Success()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()));
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.True(mod.Success);
//        }

//        // test: ChangeVariationFromLineItemToProduct handles ShoppingCartLockedException
//        [Fact]
//        public void ChangeVariation_ShoppingCartLockedException()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()))
//              .Throws(new ShoppingCartLockedException());
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartAdapter.ErrMsgAddprodCartLocked,
//                                            "CartLocked Message");

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("CartLocked Message", mod.ErrorMessages.First());
//        }

//        // test: ChangeVariationFromLineItemToProduct handles OutOfStockException by calling catalog service to get title for the error message
//        [Fact]
//        public void ChangeVariation_OutOfStockException()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()))
//              .Throws(new OutOfStockException());
//            var prod = new Product {Title = "prodtitle"};
//            _catalogAdapter.Setup(c => c.TryGetProduct(It.IsAny<string>(), out prod)).Returns(true);
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgChangeVariationGenericProduct,
//                                            "Generic Product Name");
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgChangeVariationProductOutOfStock,
//                                            "{0} ChangeVariation OutOfStock Message");

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Contains("prodtitle", mod.ErrorMessages.First());
//            Assert.Contains("ChangeVariation OutOfStock Message", mod.ErrorMessages.First());
//        }

//        // test: ChangeVariationFromLineItemToProduct handles OutOfStockException by calling catalog service to get title for the error message, but gets a null title
//        [Fact]
//        public void ChangeVariation_OutOfStockException_NullTitle()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()))
//              .Throws(new OutOfStockException());
//            var prod = new Product {Title = null};
//            _catalogAdapter.Setup(c => c.TryGetProduct(It.IsAny<string>(), out prod)).Returns(true);
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgChangeVariationGenericProduct,
//                                            "Generic Product Name");
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgChangeVariationProductOutOfStock,
//                                            "{0} ChangeVariation OutOfStock Message");

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Contains("Generic Product Name", mod.ErrorMessages.First());
//            Assert.Contains("ChangeVariation OutOfStock Message", mod.ErrorMessages.First());
//        }

//        // test: ChangeVariationFromLineItemToProduct handles OutOfStockException by calling catalog service to get title for the error message, but gets an exception
//        [Fact]
//        public void ChangeVariation_OutOfStockException_Exception()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>()))
//              .Throws(new OutOfStockException());
//            _catalogAdapter.Setup(c => c.GetProduct(It.IsAny<string>())).Throws(new Exception());
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgChangeVariationFailed,
//                                            "ChangeVariation Exception");

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("ChangeVariation Exception", mod.ErrorMessages.First());
//        }

//        // test: ChangeVariationFromLineItemToProduct handles Exception
//        [Fact]
//        public void ChangeVariation_Exception()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.ChangeVariationFromLineItemIdToProductId(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
//            var chg = new ChangeVariationModel { LineItemId = "123", ProductId = "456" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgChangeVariationFailed,
//                                            "Exception Message");

//            // test
//            var vres = ctrl.ChangeVariationFromLineItemToProduct(chg) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("Exception Message", mod.ErrorMessages.First());
//        }
//        #endregion

//        #region UpdateShipping

//        // test: UpdateShipping silently does nothing on empty carrier code
//        [Fact]
//        public void UpdateShipping_Nothing()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>())).Throws(new Exception());
//            var upd = new UpdateShippingModel { CarrierCode = null };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.UpdateShipping(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.True(mod.Success);
//            Assert.Equal(0, mod.ErrorMessages.Count()); // didn't call service.UpdateShipping, which would have thrown exception, causing an error message to be added
//        }

//        // test: UpdateShipping calls shopping cart service UpdateShipping with appropriate params
//        [Fact]
//        public void UpdateShipping_Updates()
//        {
//            // setup
//            var carrier = string.Empty; // sensing variables
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>()))
//              .Callback<string>((cr) =>
//              {
//                  carrier = cr;
//              });
//            var upd = new UpdateShippingModel { CarrierCode = "myCarrier" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.UpdateShipping(upd);

//            // sense
//            Assert.Equal(upd.CarrierCode, carrier);
//        }

//        // test: UpdateShipping returns PartialViewResult
//        [Fact]
//        public void UpdateShipping_PartialViewResult()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>()));
//            var upd = new UpdateShippingModel { CarrierCode = "myCarrier" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.UpdateShipping(upd);

//            // sense
//            Assert.True(res is PartialViewResult, "should have returned PartialViewResult");
//        }

//        // test: UpdateShipping returns SimpleResponseViewModel
//        [Fact]
//        public void UpdateShipping_Model()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>()));
//            var upd = new UpdateShippingModel { CarrierCode = "myCarrier" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.UpdateShipping(upd) as PartialViewResult;

//            // sense
//            Assert.NotNull(vres.Model);
//            Assert.True(vres.Model is SimpleResponseViewModel, "should have returned SimpleResponseViewModel");
//        }

//        // test: UpdateShipping returns success
//        [Fact]
//        public void UpdateShipping_Success()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>()));
//            var upd = new UpdateShippingModel { CarrierCode = "myCarrier" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var vres = ctrl.UpdateShipping(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.True(mod.Success);
//        }

//        // test: UpdateShipping handles ShoppingCartLockedException
//        [Fact]
//        public void UpdateShipping_ShoppingCartLockedException()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>())).Throws(new ShoppingCartLockedException());
//            var upd = new UpdateShippingModel {CarrierCode = "myCarrier"};
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartAdapter.ErrMsgAddprodCartLocked,
//                                            "CartLocked Message");

//            // test
//            var vres = ctrl.UpdateShipping(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("CartLocked Message", mod.ErrorMessages.First());
//        }

//        // test: UpdateShipping handles Exception
//        [Fact]
//        public void UpdateShipping_Exception()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.UpdateShipping(It.IsAny<string>())).Throws(new Exception());
//            var upd = new UpdateShippingModel { CarrierCode = "myCarrier" };
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();
//            FakeResources.AddString(ShoppingCartAdapter.ErrMsgResourceType, ShoppingCartPartController.ErrMsgUpdateShippingFailed,
//                                            "Exception Message");

//            // test
//            var vres = ctrl.UpdateShipping(upd) as PartialViewResult;
//            var mod = vres.Model as SimpleResponseViewModel;

//            // sense
//            Assert.NotNull(mod);
//            Assert.False(mod.Success);
//            Assert.Equal(1, mod.ErrorMessages.Count());
//            Assert.Equal("Exception Message", mod.ErrorMessages.First());
//        }
//        #endregion

//        #region GetUpdatedCrossSell
//        // test: GetUpdatedCrossSell() renders model from empty cart
//        [Fact]
//        public void GetUpdatedCrossSell_RendersFromEmptyCart()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            _catalogAdapter.Setup(c => c.GetPromotionWithProducts("myEmptyCartPromoId"))
//                           .Returns(new PromotionResultWithProducts
//                               {
//                                   Count = 1,
//                                   Name = "myEmptyCartPromoName",
//                                   OfferResults = new[]
//                                       {
//                                           new OfferResult
//                                               {
//                                                   Attributes = new ExtendedAttribute[0],
//                                                   Id = "id",
//                                                   OfferImages = new string[0]
//                                               }
//                                       }
//                               });
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.GetUpdatedCrossSell("myView", "myPromoId", "myEmptyCartPromoId", "myTitle", 0);

//            // sense
//            var vres = res as PartialViewResult;
//            Assert.Equal("myView", vres.ViewName);
//            var mod = vres.Model as CrossSellViewModel;
//            Assert.Equal("myEmptyCartPromoName", mod.Name);
//        }

//        // test: GetUpdatedCrossSell() renders model from non-empty cart
//        [Fact]
//        public void GetUpdatedCrossSell_RendersFromNonEmptyCart()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart())
//                                .Returns(new ShoppingCart
//                                    {
//                                        LineItems = new[]
//                                            {
//                                                new ShoppingCartLineItem
//                                                    {
//                                                        Product = new ShoppingCartProduct {Id = "prodId"}
//                                                    }
//                                            }
//                                    });
//            _catalogAdapter.Setup(c => c.GetPromotionWithProductsByDrivingProduct("myPromoId", "prodId"))
//                           .Returns(new PromotionResultWithProducts
//                           {
//                               Count = 1,
//                               Name = "myPromoName",
//                               OfferResults = new OfferResult[0]
//                           });

//            _catalogAdapter.Setup(c => c.GetPromotionWithProducts("myEmptyCartPromoId"))
//                           .Returns(new PromotionResultWithProducts
//                           {
//                               Count = 1,
//                               Name = "myEmptyCartPromoName",
//                               OfferResults = new[]
//                                       {
//                                           new OfferResult
//                                               {
//                                                   Attributes = new ExtendedAttribute[0],
//                                                   Id = "id",
//                                                   OfferImages = new string[0]
//                                               }
//                                       }
//                           });

//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.GetUpdatedCrossSell("myView", "myPromoId", "myEmptyCartPromoId", "myTitle", 4);

//            // sense
//            var vres = res as PartialViewResult;
//            Assert.Equal("myView", vres.ViewName);
//            var mod = vres.Model as CrossSellViewModel;
//            Assert.Equal("myEmptyCartPromoName", mod.Name);
//        }

//        // test: GetUpdatedCrossSell() renders empty model for no promotions found
//        [Fact]
//        public void GetUpdatedCrossSell_RendersEmptyModelOnNoPromos()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            var ctrl =Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.GetUpdatedCrossSell("myView", "myPromoId", "myEmptyCartPromoId", "myTitle", 0);

//            // sense
//            var vres = res as PartialViewResult;
//            Assert.Equal("myView", vres.ViewName);
//            var mod = vres.Model as CrossSellViewModel;
//            Assert.True(string.IsNullOrEmpty(mod.Name));
//            Assert.Equal(0, mod.Offers.Count);
//        }

//        // test: GetUpdatedCrossSell() builds CurrentItem
//        [Fact]
//        public void GetUpdatedCrossSell_CurrentItem()
//        {
//            // setup
//            _shoppingCartAdapter.Setup(c => c.GetShoppingCart()).Returns(new ShoppingCart());
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<MyShoppingCartPartController>();

//            // test
//            var res = ctrl.GetUpdatedCrossSell("myView", "myPromoId", "myEmptyCartPromoId", "myTitle", 4);

//            // sense
//            var vres = res as PartialViewResult;
//            var mod = vres.Model as CrossSellViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal("myTitle", mod.Title);
//            Assert.Equal("myPromoId", mod.PromotionId);
//            Assert.Equal("myEmptyCartPromoId", mod.EmptyCartPromotionId);
//            Assert.Equal(4, mod.MaxNumberOfProducts);
//        }
//        #endregion
//    }

//    // to break dependency on cache service's use of .config file
//    public class MyProductViewModelBuilder : ProductViewModelBuilder
//    {
//        public MyProductViewModelBuilder(ICacheService cacheService, ICatalogAdapter catalogApi, ISessionInfoResolver sessionInfoResolver)
//            : base(cacheService, catalogApi, sessionInfoResolver)
//        {
//        }
//    }

//    // to break dependency on WebSession
//    public class MyShoppingCartPartController : ShoppingCartPartController
//    {
//        public MyShoppingCartPartController(ILogger logger, ICatalogAdapter catalogAdapter, IProductViewModelBuilder prodViewModelBuilder,
//                                            IShoppingCartAdapter shoppingCartAdapter,
//                                            IShoppingCartViewModelBuilder shoppingCartViewModelBuilder)
//            : base(logger, catalogAdapter, prodViewModelBuilder, shoppingCartAdapter, shoppingCartViewModelBuilder)
//        {
//            SessionId = "abc";
//        }

//        protected override TS WebSessionGet<TS>(string name)
//        {
//            return new TS();
//        }
//    }

//}
