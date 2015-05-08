//using System.Linq;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pages;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Core.Services.Logging;
//using Moq;
//using ViewModelBuilders.Catalog;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Pages
//{
//    public class BundleProductPickerControllerTests : TestBase
//    {
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly Mock<IProductViewModelBuilder> _prodViewModelBuilder = new Mock<IProductViewModelBuilder>();
//        private readonly Mock<IShoppingCartAdapter> _shoppingCartAdapter = new Mock<IShoppingCartAdapter>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
//        private readonly Mock<ILogger> _logger = new Mock<ILogger>();

//        public BundleProductPickerControllerTests()
//        {
//            DependencyRegistrar.
//                StandardDependencies()
//                               .With(_shoppingCartAdapter.Object)
//                               .With(_prodViewModelBuilder.Object)
//                               .With(_linkGenerator.Object)
//                               .With(_logger.Object);
//            WebSession.Current = _webSession.Object;
//        }

//        [Fact]
//        public void AddBundleToCart_ReturnsOfiginalOfferInResultView()
//        {
//            // setup
//            var bundleProductId = "123";
//            var productId1 = "234";
//            var productId2 = "345";
//            var offerId = "456";
//            var offerName = "MyOffer";
//            var offerInstanceId = "1";
//            var request = new BundleProductPickerAddToCartRequest
//            {
//                ProductId = bundleProductId,
//                OfferId = offerId,
//                BuyProductIds = new[] {bundleProductId, productId1, productId2}
//            };
//            _prodViewModelBuilder.Setup(c => c.GetBundleProductPickerViewModel(It.IsAny<string>(), It.IsAny<string>())).Returns(new BundleProductPickerViewModel
//            {
//                OfferId = offerId,
//                OfferName = offerName,
//                OfferInstanceId = offerInstanceId,
//                OriginalBundleGroups = new OfferBundleGroupViewModel[0],
//                FullyMandatoryBundleGroups = new OfferBundleGroupViewModel[0],
//                PartiallyMandatoryBundleGroups = new OfferBundleGroupViewModel[0],
//                OptionalBundleGroups = new OfferBundleGroupViewModel[0]
//            });
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<BundleProductPickerController>();

//            // test
//            var res = ctrl.AddBundleToCart(request).Data as BundleProductPickerAddToCartResponse;

//            // sense
//            Assert.NotNull(res);
//            var offer = res.Offer;
//            Assert.Equal(offerId, offer.OfferId);
//            Assert.Equal(offerName, offer.OfferName);
//            Assert.Equal(offerInstanceId, offer.OfferInstanceId);
//            Assert.Equal(0, offer.OriginalBundleGroups.Length);
//            Assert.Equal(0, offer.FullyMandatoryBundleGroups.Length);
//            Assert.Equal(0, offer.PartiallyMandatoryBundleGroups.Length);
//            Assert.Equal(0, offer.OptionalBundleGroups.Length);
//        }

//        [Fact]
//        public void AddBundleToCart_OnlyOrignialProductInBundle()
//        {
//            // setup
//            var bundleProductId = "123";
//            var productId1 = "234";
//            var productId2 = "345";
//            var offerId = "456";
//            var offerName = "MyOffer";
//            var offerInstanceId = "1";
//            var originalBundleGroupInstanceId = "1";
//            var originalBundleGroupName = "Original";
//            var request = new BundleProductPickerAddToCartRequest
//            {
//                ProductId = bundleProductId,
//                OfferId = offerId,
//                BuyProductIds = new[] { bundleProductId, productId1, productId2 }
//            };
//            _prodViewModelBuilder.Setup(c => c.GetBundleProductPickerViewModel(It.IsAny<string>(), It.IsAny<string>())).Returns(new BundleProductPickerViewModel
//            {
//                OfferId = offerId,
//                OfferName = offerName,
//                OfferInstanceId = offerInstanceId,
//                OriginalBundleGroups = new []
//                {
//                    new OfferBundleGroupViewModel()
//                    {
//                        BundleGroupInstanceId = originalBundleGroupInstanceId,
//                        BundleGroupName = originalBundleGroupName,
//                        Products = new []
//                        {
//                            new BundleOfferProductViewModel()
//                            {
//                                Product = new BundleOfferProductInfo
//                                {
//                                    ProductId = bundleProductId
//                                }
//                            }
//                        }
    
//                    }
//                },
//                FullyMandatoryBundleGroups = new OfferBundleGroupViewModel[0],
//                PartiallyMandatoryBundleGroups = new OfferBundleGroupViewModel[0],
//                OptionalBundleGroups = new OfferBundleGroupViewModel[0]
//            });
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<BundleProductPickerController>();

//            // test
//            var res = ctrl.AddBundleToCart(request).Data as BundleProductPickerAddToCartResponse;

//            // sense
//            Assert.False(res.AddedToCart);
//            Assert.Equal(2, res.Errors.ProductIdsNotInAnyGroup.Length);
//            Assert.True(res.Errors.ProductIdsNotInAnyGroup.Any(e => e == productId1));
//            Assert.True(res.Errors.ProductIdsNotInAnyGroup.Any(e => e == productId2));
//        }
//    }
//}
