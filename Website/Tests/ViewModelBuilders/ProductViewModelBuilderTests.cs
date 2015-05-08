//using System;
//using System.Collections.Generic;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Common;
//using DigitalRiver.CloudLink.Commerce.Api.Configuration;
//using DigitalRiver.CloudLink.Commerce.Api.Session;
//using DigitalRiver.CloudLink.Commerce.Api.Utils;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout;
//using DigitalRiver.CloudLink.Core.Services.Cache;
//using DigitalRiver.CloudLink.Core.Services.Configuration;
//using Moq;
//using ViewModelBuilders.Catalog;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.ViewModelBuilders
//{
//    public class ProductViewModelBuilderTests : TestBase
//    {
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
//        private readonly Mock<ICacheKeyResolver> _cacheKeyResolver = new Mock<ICacheKeyResolver>();
//        private readonly Mock<ICacheService> _cacheService = new Mock<ICacheService>();
//        private readonly Mock<ICache<Products>> _productCache = new Mock<ICache<Products>>();
//        private readonly Mock<ICache<CategoryViewModel>> _categoryCache = new Mock<ICache<CategoryViewModel>>();
//        private readonly Mock<ICache<IEnumerable<Breadcrumb>>> _breadcrumbCache = new Mock<ICache<IEnumerable<Breadcrumb>>>();
//        private readonly Mock<ICache<IEnumerable<ProductMediaViewModel>>> _productMediaCache = new Mock<ICache<IEnumerable<ProductMediaViewModel>>>();
//        private readonly Mock<IShoppingCartAdapter> _shoppingCartAdapter = new Mock<IShoppingCartAdapter>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<IConfigKeyResolver> _configKeyResolver = new Mock<IConfigKeyResolver>();
//        private readonly Mock<IConfigurationService> _configurationService = new Mock<IConfigurationService>();
//        private readonly Mock<ISessionInfoResolver> _sessionInfoResolver = new Mock<ISessionInfoResolver>();

//        public ProductViewModelBuilderTests()
//        {
//            DependencyRegistrar
//                 .StandardDependencies()
//                 .With(_linkGenerator.Object)
//                 .With(_shoppingCartAdapter.Object)
//                 .With(_catalogAdapter.Object)
//                 .With(_cacheKeyResolver.Object)
//                 .WithMetadata(_cacheService.Object, "CacheService")
//                 .WithSingleton<IProductViewModelBuilder, ProductViewModelBuilder>()
//                 .With<ProductVariationUtils>()
//                 .With(_configKeyResolver.Object)
//                 .With<ProductVariationsConfig>()
//                 .With(_configurationService.Object)
//                 .With(_sessionInfoResolver.Object)
//                 .With(_productMediaCache.Object);
//            ConfigurationService.Register(_configurationService.Object);

//            CreateWebSession();
//        }

//        private void CreateWebSession()
//        {
//            WebSession.Current = _webSession.Object;
//        }

//        #region GetProductDetail
//        // test: GetProductDetail returns null if the productId parameter is null
//        [Fact]
//        public void GetProductDetail_NullProductId()
//        {
//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetProductDetail(null);

//            // sense
//            Assert.Null(model);
//        }

//        // test: GetProductDetail passes the productId parameter to the unerlying catalog services' GetProductDetail method 
//        [Fact]
//        public void GetProductDetail_NonNullProductId_CorrectParameter()
//        {
//            // setup
//            const string productId = "Product1";
//            var prodId = String.Empty;
//            CacheKey cacheKey;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            _catalogAdapter.Setup(cs => cs.GetProductDetail(It.IsAny<string>()))
//                 .Returns((Product)null)
//                 .Callback<string>(pId => prodId = pId);

//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetProductDetail(productId);

//            // sense
//            Assert.Equal(prodId, productId);
//        }

//        // test: GetProductDetail returns a ProductDetailViewModel populated with relevant stuff copied from the identified product
//        [Fact]
//        public void GetProductDetail_Model()
//        {
//            // setup
//            SetupCacheService();
//            const string productId = "Product1";
//            var attributes = new ExtendedAttribute[0];
//            const string description = "Description";
//            var detailImages = new ProductDetailImage[0];
//            var discount = new Money { Amount = 10.0M, CurrencyCode = "USD" };
//            const bool hasVariations = false;
//            var images = new ProductImages();
//            var inventory = new ProductInventory();
//            const bool isDisplayable = true;
//            const bool isPurchasable = true;
//            const string longDescription = "LongDescription";
//            const string manufacturerName = "ManufacturerName";
//            const string manufacturerPartNumber = "ManufacturerPartNumber";
//            const string metaDescription = "MetaDescription";
//            var priceLists = new PriceList[0];
//            var salePrice = new Money { Amount = 9.99M, CurrencyCode = "USD" };
//            const string sku = "Sku";
//            const string title = "Title";
//            var unitPrice = new Money { Amount = 10.0M, CurrencyCode = "USD" };
//            var variations = new ProductVariations();
//            CacheKey cacheKey;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            _catalogAdapter.Setup(cs => cs.GetProductDetail(It.IsAny<string>()))
//                 .Returns(new Product
//                 {
//                     Attributes = attributes,
//                     Description = description,
//                     DetailImages = detailImages,
//                     Discount = discount,
//                     HasVariations = hasVariations,
//                     Id = productId,
//                     Images = images,
//                     Inventory = inventory,
//                     IsDisplayable = isDisplayable,
//                     IsPurchasable = isPurchasable,
//                     LongDescription = longDescription,
//                     ManufacturerName = manufacturerName,
//                     ManufacturerPartNumber = manufacturerPartNumber,
//                     MetaDescription = metaDescription,
//                     PriceLists = priceLists,
//                     SalePrice = salePrice,
//                     Sku = sku,
//                     Title = title,
//                     UnitPrice = unitPrice,
//                     Variations = variations
//                 });

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetProductDetail(productId);

//            // sense
//            Assert.Equal(attributes, model.Product.Attributes);
//            Assert.Equal(description, model.Product.Description);
//            Assert.Equal(detailImages, model.Product.DetailImages);
//            Assert.Equal(discount, model.Product.Discount);
//            Assert.Equal(hasVariations, model.Product.HasVariations);
//            Assert.Equal(productId, model.Product.Id);
//            Assert.Equal(images, model.Product.Images);
//            Assert.Equal(inventory, model.Product.Inventory);
//            Assert.Equal(isDisplayable, model.Product.IsDisplayable);
//            Assert.Equal(isPurchasable, model.Product.IsPurchasable);
//            Assert.Equal(longDescription, model.Product.LongDescription);
//            Assert.Equal(manufacturerName, model.Product.ManufacturerName);
//            Assert.Equal(manufacturerPartNumber, model.Product.ManufacturerPartNumber);
//            Assert.Equal(metaDescription, model.Product.MetaDescription);
//            Assert.Equal(priceLists, model.Product.PriceLists);
//            Assert.Equal(salePrice, model.Product.SalePrice);
//            Assert.Equal(sku, model.Product.Sku);
//            Assert.Equal(title, model.Product.Title);
//            Assert.Equal(unitPrice, model.Product.UnitPrice);
//            Assert.Equal(variations, model.Product.Variations);
//        }
//        #endregion

//        #region GetPromotion

//        //  test: GetPromotion passes its input paramter on the the underlying catalog service call faithfully
//        [Fact]
//        public void GetPromotion_ByID_PassesParm()
//        {
//            //  setup
//            string promotionId = "PromotionID1", pid = String.Empty;
//            _catalogAdapter.Setup(cs => cs.GetPromotionWithProducts(It.IsAny<string>()))
//                 .Returns((PromotionResultWithProducts)null)
//                 .Callback<string>(pId => pid = pId);

//            // test
//            try
//            {
//                Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotion(promotionId);
//            }
//            catch (Exception)
//            {
//                Assert.False(true, "Should not have thrown an exception.");
//            }

//            // sense
//            Assert.Equal(pid, promotionId);
//        }

//        //  test: GetPromotion returns a null model if the given promotionId yield a null promotion from the catalog service
//        [Fact]
//        public void GetPromotion_ByID_Null()
//        {
//            //  setup
//            _catalogAdapter.Setup(cs => cs.GetPromotion(It.IsAny<string>())).Returns((PromotionResult)null);
//            const string promotionId = "PromotionID1";

//            // test
//            CrossSellViewModel model = null;
//            try
//            {
//                model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotion(promotionId);
//            }
//            catch (Exception)
//            {
//                Assert.False(true, "Should not have thrown an exception.");
//            }

//            // sense
//            Assert.Null(model);
//        }

//        //  test: GetPromotion returns a null model if the given promotionId yield a null promotion from the catalog service
//        [Fact]
//        public void GetPromotion_ByID_Exception()
//        {
//            //  setup
//            _catalogAdapter.Setup(cs => cs.GetPromotion(It.IsAny<string>())).Throws<Exception>();
//            const string promotionId = "PromotionID1";

//            // test
//            CrossSellViewModel model = null;
//            try
//            {
//                model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotion(promotionId);
//            }
//            catch (Exception)
//            {
//                Assert.False(true, "Should not have thrown an exception.");
//            }

//            // sense
//            Assert.Null(model);
//        }

//        //  test: GetPromotion returns a null model if the given promotionId yield a null promotion from the catalog service
//        [Fact]
//        public void GetPromotion_ByID_ValidButNoOffers()
//        {
//            //  setup
//            _catalogAdapter.Setup(cs => cs.GetPromotion(It.IsAny<string>())).Returns(new PromotionResult { Count = 0, Name = "AnyName" });
//            const string promotionId = "PromotionID1";

//            // test
//            CrossSellViewModel model = null;
//            try
//            {
//                model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotion(promotionId);
//            }
//            catch (Exception)
//            {
//                Assert.False(true, "Should not have thrown an exception.");
//            }

//            // sense
//            Assert.Null(model);
//        }

//        //  test: GetPromotion returns a null model if the given promotionId yield a null promotion from the catalog service
//        [Fact]
//        public void GetPromotion_ByID_ValidWithOffers()
//        {
//            //  setup
//            string promotionName = "PromotionName1";
//            _catalogAdapter.Setup(cs => cs.GetPromotionWithProducts(It.IsAny<string>()))
//                .Returns(new PromotionResultWithProducts { Count = 2, Name = promotionName, OfferResults = new[]
//                {
//                    new OfferResult { Id = "Offer1", Products = new[] {new Product {Id = "Product1"}}},
//                    new OfferResult { Id = "Offer2", Products = new[] {new Product {Id = "Product1"}}}
//                }});
//            const string promotionId = "PromotionID1";

//            // test
//            CrossSellViewModel model = null;
//            try
//            {
//                model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotion(promotionId);
//            }
//            catch (Exception)
//            {
//                Assert.False(true, "Should not have thrown an exception.");
//            }

//            // sense
//            Assert.Equal(promotionName, model.Name);
//            Assert.Equal(2, model.Offers.Count);
//            Assert.Equal(1, model.Offers[0].Products.Length);
//        }

//        #endregion

//        #region GetPromotionByDrivingProduct
//        // test: GetPromotionByDrivingProduct handles exception
//        [Fact]
//        public void GetPromotionByDrivingProduct_Exception()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetPromotionByDrivingProduct("1", "2"))
//                           .Throws(new Exception("oops"));

//            // test
//            CrossSellViewModel model = null;
//            try
//            {
//                model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotionByDrivingProduct("1", "2");
//            }
//            catch (Exception)
//            {
//                Assert.False(true, "Should not have thrown an exception.");
//            }

//            // sense
//            Assert.Null(model);
//        }

//        // test: GetPromotionByDrivingProduct builds model from promotions
//        [Fact]
//        public void GetPromotionByDrivingProduct_Model()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetPromotionWithProductsByDrivingProduct("1", "2"))
//                           .Returns(new PromotionResultWithProducts
//                               {
//                                   Count = 2,
//                                   Name = "res",
//                                   OfferResults = new[]
//                                   {
//                                       new OfferResult {Id = "O1", Products = new[] {new Product {Description = "O1 prod"}}},
//                                       new OfferResult {Id = "O2", Products = new[]
//                                           {
//                                               new Product {Description = "O2 prod 1"}, new Product {Description = "O2 prod 2"}
//                                           }}
//                                   }
//                               });

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotionByDrivingProduct("1", "2");

//            // sense
//            Assert.NotNull(model);
//            Assert.Equal("res", model.Name);
//            Assert.Equal(2, model.Offers.Count);
//            Assert.Equal(1, model.Offers[0].Products.Length);
//            Assert.Equal(2, model.Offers[1].Products.Length);
//            Assert.Equal("O1 prod", model.Offers[0].Products[0].Description);
//            Assert.Equal("O2 prod 1", model.Offers[1].Products[0].Description);
//            Assert.Equal("O2 prod 2", model.Offers[1].Products[1].Description);
//        }
//        #endregion

//        #region GetPromotionByDrivingProducts
//        // test: GetPromotionByDrivingProducts behaves the same as GetPromotionByDrivingProduct if null product ids specified
//        [Fact]
//        public void GetPromotionByDrivingProducts_NullProductIds()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetPromotionWithProductsByDrivingProduct("1", null))
//                           .Returns(new PromotionResultWithProducts
//                               {
//                                   Count = 1,
//                                   Name = "res",
//                                   OfferResults = new[] { new OfferResult { Id = "O1", Products = new[] { new Product { Description = "O1 prod" } } } }
//                               });

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotionByDrivingProducts("1");

//            // sense
//            Assert.NotNull(model);
//            Assert.Equal("res", model.Name);
//            // don't have to repeat sensing already done for GetPromotionByDrivingProduct, since we now know it called GetPromotionByDrivingProduct
//        }

//        // test: GetPromotionByDrivingProducts behaves the same as GetPromotionByDrivingProduct if empty product ids specified
//        [Fact]
//        public void GetPromotionByDrivingProducts_EmptyProductIds()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetPromotionWithProductsByDrivingProduct("1", null))
//                           .Returns(new PromotionResultWithProducts
//                           {
//                               Count = 1,
//                               Name = "res",
//                               OfferResults = new[] { new OfferResult { Id = "O1", Products = new[] { new Product { Description = "O1 prod" } } } }
//                           });

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetPromotionByDrivingProducts("1", new string[0]);

//            // sense
//            Assert.NotNull(model);
//            Assert.Equal("res", model.Name);
//            // don't have to repeat sensing already done for GetPromotionByDrivingProduct, since we now know it called GetPromotionByDrivingProduct
//        }

//        // test: GetPromotionByDrivingProducts forms model when both promotion and product ids specified
//        [Fact]
//        public void GetPromotionByDrivingProducts_ProductIds()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetPromotionWithProductsByDrivingProduct("1", "prod1"))
//                           .Returns(new PromotionResultWithProducts
//                               {
//                                   Count = 1,
//                                   Name = "res",
//                                   OfferResults = new[] { new OfferResult { Id = "O1 prod1", Products = new[] { new Product { Description = "O1 prod1" } } } }
//                               });
//            _catalogAdapter.Setup(c => c.GetPromotionWithProductsByDrivingProduct("1", "prod2"))
//                           .Returns(new PromotionResultWithProducts
//                               {
//                                   Count = 1,
//                                   Name = "res",
//                                   OfferResults = new[]
//                                   {
//                                       new OfferResult { Id = "O1 prod2", Products = new[] { new Product { Description = "O1 prod2" } } },
//                                       new OfferResult { Id = "O2 prod2", Products = new[] { new Product { Description = "O2 prod2" } } }
//                                   }
//                               });

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>()
//                                          .GetPromotionByDrivingProducts("1", new [] {"prod1", "prod2"});

//            // sense
//            Assert.NotNull(model);
//            Assert.Equal("res", model.Name);
//            Assert.Equal(3, model.Offers.Count);
//            Assert.Equal(1, model.Offers[0].Products.Length);
//            Assert.Equal(1, model.Offers[1].Products.Length);
//            Assert.Equal(1, model.Offers[2].Products.Length);
//            Assert.Equal("O1 prod1", model.Offers[0].Products[0].Description);
//            Assert.Equal("O1 prod2", model.Offers[1].Products[0].Description);
//            Assert.Equal("O2 prod2", model.Offers[2].Products[0].Description);
//        }

//        // test: GetPromotionByDrivingProducts handles exception
//        [Fact]
//        public void GetPromotionByDrivingProducts_Exception()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetPromotionByDrivingProduct("1", "prod1")).Throws(new Exception("dang"));

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>()
//                                          .GetPromotionByDrivingProducts("1", new [] { "prod1" });

//            // sense
//            Assert.Null(model);
//        }

//        #endregion

//        #region GetBundleProductPickerViewModel

//        // test: GetBundleProductPickerViewModel properly groups and sorts bundle groups
//        [Fact]
//        public void GetBundleProductPickerViewModel()
//        {
//            // setup
//            _catalogAdapter.Setup(c => c.GetCustomBundleOffer(It.IsAny<string>(), It.IsAny<string>()))
//                           .Returns(new CustomBundleOffer
//                           {
//                                OfferInstanceId = "1",
//                                OfferBundleGroups = new []
//                                {
//                                    new OfferBundleGroup
//                                    {
//                                        BundleGroupInstanceId = "1",
//                                        BundleGroupName = "OptionalBundleGroup",
//                                        Description = "Optional Bundle Group",
//                                        DisplayOrder = 3,
//                                        DisplayName = "Step On It!",
//                                        Mandatory = false,
//                                        MaxProductQuantity = 3,
//                                        MinProductQuantity = 1,
//                                        Products = new []
//                                        {
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 1,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 47",
//                                                    ProductId = "47"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 2,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 98",
//                                                    ProductId = "98"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 3,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 99",
//                                                    ProductId = "99"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 4,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 100",
//                                                    ProductId = "100"
//                                                }
//                                            }
//                                        }
//                                    },
//                                    new OfferBundleGroup
//                                    {
//                                        BundleGroupInstanceId = "2",
//                                        BundleGroupName = "PartiallyMandatoryBundleGroup",
//                                        Description = "Partially Mandatory Bundle Group",
//                                        DisplayOrder = 2,
//                                        DisplayName = "Step It Up!",
//                                        Mandatory = true,
//                                        MaxProductQuantity = 2,
//                                        MinProductQuantity = 2,
//                                        Products = new []
//                                        {
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 1,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 200",
//                                                    ProductId = "200"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 2,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 201",
//                                                    ProductId = "201"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 3,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 202",
//                                                    ProductId = "202"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 4,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 203",
//                                                    ProductId = "203"
//                                                }
//                                            }
//                                        }
//                                    },
//                                    new OfferBundleGroup
//                                    {
//                                        BundleGroupInstanceId = "3",
//                                        BundleGroupName = "OriginalBundleGroup",
//                                        Description = "Original Bundle Group",
//                                        DisplayOrder = 1,
//                                        DisplayName = "Step Down!",
//                                        Mandatory = true,
//                                        MaxProductQuantity = 1,
//                                        MinProductQuantity = 1,
//                                        Products = new []
//                                        {
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 1,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 1",
//                                                    ProductId = "1"
//                                                }
//                                            }
//                                        }
//                                    },
//                                    new OfferBundleGroup
//                                    {
//                                        BundleGroupInstanceId = "4",
//                                        BundleGroupName = "FullyMandatoryBundleGroup",
//                                        Description = "Fully Mandatory Bundle Group",
//                                        DisplayOrder = 5,
//                                        DisplayName = "Step Over!",
//                                        Mandatory = true,
//                                        MaxProductQuantity = 2,
//                                        MinProductQuantity = 2,
//                                        Products = new []
//                                        {
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 1,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 299",
//                                                    ProductId = "299"
//                                                }
//                                            },
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 2,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 300",
//                                                    ProductId = "300"
//                                                }
//                                            }
//                                        }
//                                    },
//                                    new OfferBundleGroup
//                                    {
//                                        BundleGroupInstanceId = "5",
//                                        BundleGroupName = "AnotherFullyMandatoryBundleGroup",
//                                        Description = "Another Fully Mandatory Bundle Group",
//                                        DisplayOrder = 4,
//                                        DisplayName = "Step Under!",
//                                        Mandatory = true,
//                                        MaxProductQuantity = 1,
//                                        MinProductQuantity = 1,
//                                        Products = new []
//                                        {
//                                            new BundleOfferProduct
//                                            {
//                                                DisplayOrder = 1,
//                                                Product = new BundleOfferProductInfo
//                                                {
//                                                    DisplayableProduct = true,
//                                                    DisplayName = "Product 399",
//                                                    ProductId = "399"
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                           });
//            SetupCacheService();
//            CacheKey cacheKey = new MyCacheKey("gorp");
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<IProductViewModelBuilder>().GetBundleProductPickerViewModel("1", "1");

//            // sense
//            Assert.NotNull(model);
//            Assert.Equal(1, model.OriginalBundleGroups.Length);
//            Assert.Equal(2, model.FullyMandatoryBundleGroups.Length);
//            Assert.Equal("AnotherFullyMandatoryBundleGroup", model.FullyMandatoryBundleGroups[0].BundleGroupName);
//            Assert.Equal(1, model.PartiallyMandatoryBundleGroups.Length);
//            Assert.Equal(1, model.OptionalBundleGroups.Length);
//        }

//        #endregion

//        #region private stuff
//        /*
//        private void AssertItemsEqual<T>(T[] expected, T[] actual, IEqualityComparer<T> comparer)
//        {
//            Assert.Equal(expected.Length, actual.Length);
//            for (var i = 1; i < expected.Length; i++)
//            {
//                Assert.Equal(expected[i], actual[i], comparer);
//            }
//        }
//        */

//        private void SetupCacheService()
//        {
//            _cacheService.Setup(cs => cs.GetCache<Products>(It.IsAny<CacheConfig>())).Returns(_productCache.Object);
//            _cacheService.Setup(cs => cs.GetCache<CategoryViewModel>(It.IsAny<CacheConfig>())).Returns(_categoryCache.Object);
//            _cacheService.Setup(cs => cs.GetCache<IEnumerable<Breadcrumb>>(It.IsAny<CacheConfig>())).Returns(_breadcrumbCache.Object);
//            _cacheService.Setup(cs => cs.GetCache<IEnumerable<ProductMediaViewModel>>(It.IsAny<CacheConfig>())).Returns(_productMediaCache.Object);
//        }

//        class MyCacheKey : CacheKey
//        {
//            public MyCacheKey(string name)
//            {
//                _parameters.Add(name);
//            }
//        }
//        #endregion
//    }
//}
