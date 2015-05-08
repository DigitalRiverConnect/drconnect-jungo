//using System;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Api.Common;
//using DigitalRiver.CloudLink.Commerce.Api.Configuration;
//using DigitalRiver.CloudLink.Commerce.Api.Session;
//using DigitalRiver.CloudLink.Commerce.Api.Utils;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.DregsToResolve;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using DigitalRiver.CloudLink.Core.Services.Cache;
//using Moq;
//using ViewModelBuilders.Catalog;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.ViewModelBuilders
//{
//    public class CategoryViewModelBuilderTests : TestBase
//    {
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly Mock<ICacheService> _cacheService = new Mock<ICacheService>();
//        private readonly Mock<ICache<Products>> _productCache = new Mock<ICache<Products>>();
//        private readonly Mock<ICacheKeyResolver> _cacheKeyResolver = new Mock<ICacheKeyResolver>();
//        private readonly Mock<ICache<CategoryViewModel>> _categoryCache = new Mock<ICache<CategoryViewModel>>();
//        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();
//        private readonly Mock<ISessionInfoResolver> _sessionInfoResolver = new Mock<ISessionInfoResolver>();

//        public CategoryViewModelBuilderTests()
//        {
//            DependencyRegistrar
//                .StandardDependencies()
//                //.With(_linkGenerator.Object)
//                //.With(_shoppingCartAdapter.Object)
//                .With(_catalogAdapter.Object)
//                .With(_cacheKeyResolver.Object)
//                .WithMetadata(_cacheService.Object, "CacheService")
//                .WithSingleton<ICategoryViewModelBuilder, CategoryViewModelBuilder>()
//                .With<ProductVariationUtils>()
//                //.With(_configKeyResolver.Object)
//                .With<ProductVariationsConfig>()
//                 //.With(_configurationService.Object)
//                 .With(_sessionInfoResolver.Object)
//                 //.With(_productMediaCache.Object)
//                 ;
//            //ConfigurationService.Register(_configurationService.Object);

//            CreateWebSession();
//        }

//        private void CreateWebSession()
//        {
//            WebSession.Current = _webSession.Object;
//        }

//        #region GetCategoryAsync

//        // test: GetCategoryAsync returns a null model if the categoryId is null
//        [Fact]
//        public void GetCategory_NullCategoryID()
//        {
//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoryAsync(0).Result;

//            // sense
//            Assert.Null(model);
//        }

//        // test: GetCategoryAsync returns a null model if the categoryId is invalid and that result was previously cached
//        [Fact]
//        public void GetCategory_NonNullInvalidCategoryID_InCache()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(true);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoryAsync(0).Result;

//            // sense
//            Assert.Null(model);
//        }

//        // test: GetCategoryAsync returns a null model if the categoryId is invalid and not previously cached
//        [Fact]
//        public void GetCategory_NonNullInvalidCategoryID_NotInCache()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                 .Returns((ProductCategory)null);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoryAsync(0).Result;

//            // sense
//            Assert.Null(model);
//        }

//        // test: GetCategoryAsync passes the categoryId parameter to the underlying category service's GetCategoryAsync method
//        [Fact]
//        public void GetCategory_NonNullCategoryID_PassedCorrectly()
//        {
//            // setup
//            SetupCacheService();
//            const long categoryId = 1;
//            var catId = String.Empty;
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                 .Returns(new ProductCategory())    // this makes the category id invalid
//                 .Callback<string>(cId => catId = cId);

//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoryAsync(categoryId).Wait();

//            // sense
//            Assert.Equal("0", catId);
//        }

//        // test: GetCategoryAsync returns a CategoryViewModel with appropriate contents if given a valid categoryId
//        [Fact]
//        public void GetCategory_ValidCategoryID()
//        {
//            // setup
//            SetupCacheService();
//            const long categoryId = 1;
//            const string displayName = "DisplayName";
//            const string categoryImage = "SomeURL";
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                .Returns(new ProductCategory
//                {
//                    CategoryId = categoryId,
//                    DisplayName = displayName,
//                    CategoryImage = categoryImage,
//                });

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoryAsync(categoryId).Result;

//            // sense
//            Assert.Equal(categoryId, model.CategoryId);
//            Assert.Equal(displayName, model.DisplayName);
//            Assert.Equal(categoryImage, model.Image);
//            Assert.Equal(categoryId, model.ParentCategoryId);
//            Assert.Equal(0, model.Count);
//            Assert.Equal(0, model.ProductCount);
//        }

//        // test: GetCategoryAsync caches a CategoryViewModel with appropriate contents if given a valid categoryId
//        [Fact]
//        public void GetCategory_ValidCategoryID_Caches()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            const string displayName = "DisplayName";
//            const string categoryImage = "SomeURL";
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvModel, cachedViewModel = null;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvModel)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                .Returns(new ProductCategory
//                {
//                    CategoryId = categoryId,
//                    DisplayName = displayName,
//                    CategoryImage = categoryImage,
//                });
//            _categoryCache.Setup(cc => cc.Add(null, It.IsAny<CategoryViewModel>()))
//              .Callback<CacheKey, CategoryViewModel>((ck, cvm) => cachedViewModel = cvm);


//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoryAsync(categoryId);

//            // sense
//            Assert.Equal(categoryId, cachedViewModel.CategoryId);
//            Assert.Equal(displayName, cachedViewModel.DisplayName);
//            Assert.Equal(categoryImage, cachedViewModel.Image);
//            Assert.Equal(categoryId, cachedViewModel.ParentCategoryId);
//            Assert.Equal(0, cachedViewModel.Count);
//            Assert.Equal(0, cachedViewModel.ProductCount);
//        }

//        #endregion

//        #region GetCategoriesAsync

//        // test: GetCategoriesAsync with no parameters (not previously cached), returns a "Root" CategoryViewModel
//        [Fact]
//        public void GetCategories_NoParameters_NotCached()
//        {
//            // setup
//            SetupCacheService();
//            CacheKey cacheKey;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            _catalogAdapter.Setup(cs => cs.GetCategories(null, true)).Returns(new[]
//                {
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory1",
//                        },
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory2",
//                        },
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory3",
//                        }
//                });
//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoriesAsync();

//            // sense
//            Assert.Equal("0000000", model.CategoryId);
//            Assert.Equal(3, model.Count);
//            Assert.Equal("Root", model.DisplayName);
//            Assert.Equal("", model.Image);
//            Assert.Equal(null, model.ParentCategoryId);
//            Assert.Equal(0, model.ProductCount);
//        }

//        // test: GetCategoriesAsync with no parameters (not previously cached), caches a "Root" CategoryViewModel
//        [Fact]
//        public void GetCategories_NoParameters_NotCached_Caches()
//        {
//            // setup
//            CategoryViewModel cachedViewModel = null;
//            SetupCacheService();
//            CacheKey cacheKey;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            _catalogAdapter.Setup(cs => cs.GetCategories(null, true)).Returns(new[]
//                {
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory1",
//                        },
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory2",
//                        },
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory3",
//                        }
//                });
//            _categoryCache.Setup(cc => cc.Add(null, It.IsAny<CategoryViewModel>()))
//                          .Callback<CacheKey, CategoryViewModel>((ck, cvm) => cachedViewModel = cvm);
//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoriesAsync();

//            // sense
//            Assert.Equal("0000000", cachedViewModel.CategoryId);
//            Assert.Equal(3, cachedViewModel.Count);
//            Assert.Equal("Root", cachedViewModel.DisplayName);
//            Assert.Equal("", cachedViewModel.Image);
//            Assert.Equal(null, cachedViewModel.ParentCategoryId);
//            Assert.Equal(0, cachedViewModel.ProductCount);
//        }

//        // test: GetCategoriesAsync with no parameters (previously cached), returns a "Root" CategoryViewModel
//        [Fact]
//        public void GetCategories_NoParameters_Cached()
//        {
//            // setup
//            var cachedViewModel = new CategoryViewModel
//            {
//                CategoryId = "0000000",
//                DisplayName = "Root",
//                Image = "",
//                Keywords = "unitTest",
//                Attributes = new ExtendedAttribute[1] {
//                        new ExtendedAttribute {
//                            Name = "TestAttr",
//                            Type = "integer",
//                            Value = "123"
//                        }
//                    }
//            };
//            SetupCacheService();
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            _categoryCache.Setup(cc => cc.TryGet(cacheKey, out cachedViewModel)).Returns(true);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoriesAsync();

//            // sense
//            Assert.Equal("0000000", cachedViewModel.CategoryId);
//            Assert.Equal(0, cachedViewModel.Count);
//            Assert.Equal("Root", cachedViewModel.DisplayName);
//            Assert.Equal("", cachedViewModel.Image);
//            Assert.Equal(null, cachedViewModel.ParentCategoryId);
//            Assert.Equal(0, cachedViewModel.ProductCount);
//            Assert.Equal("unitTest", cachedViewModel.Keywords);
//            Assert.Equal(1, cachedViewModel.Attributes.Length);
//            var attr = cachedViewModel.Attributes[0];
//            Assert.Equal("TestAttr", attr.Name);
//            Assert.Equal("integer", attr.Type);
//            Assert.Equal("123", attr.Value);
//        }

//        // test: GetCategoriesAsync with a valid cateogryId (not cached) returns an appropriate CategoryViewModel
//        [Fact]
//        public void GetCategories_ValidCategoryId_NotCached()
//        {
//            // setup
//            const string categoryId = "CategoryId";
//            const string categoryImage = "CategoryImage";
//            const string displayName = "DisplayName";
//            const bool hasChildren = true;
//            const string keywords = "key word";
//            var subCategories = new[]
//                {
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory1",
//                            SubCategories = new[]
//                                {
//                                    new ProductCategory
//                                    {
//                                        Attributes = null,
//                                        CategoryId = "ChildCategory11"
//                                    },
//                                    new ProductCategory
//                                    {
//                                        Attributes = null,
//                                        CategoryId = "ChildCategory12"
//                                    }
//                                },
//                        },
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory2",
//                        },
//                    new ProductCategory
//                        {
//                            Attributes = null,
//                            CategoryId = "ChildCategory3",
//                        }
//                };
//            SetupCacheService();
//            CacheKey cacheKey;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            var productCategory = new ProductCategory
//            {
//                CategoryId = categoryId,
//                Attributes = null,
//                CategoryImage = categoryImage,
//                DisplayName = displayName,
//                HasChildren = hasChildren,
//                Keywords = keywords,
//                SubCategories = subCategories,
//            };
//            _catalogAdapter.Setup(cs => cs.GetCategory(categoryId)).Returns(productCategory);
//            _catalogAdapter.Setup(cs => cs.GetCategories(categoryId, true)).Returns(subCategories);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoriesAsync(categoryId, 2);

//            // sense
//            Assert.Equal(categoryId, model.CategoryId);
//            Assert.Equal(3, model.Count);
//            Assert.Equal(displayName, model.DisplayName);
//            Assert.Equal(categoryImage, model.Image);
//            Assert.Equal(categoryId, model.ParentCategoryId);
//            Assert.Equal(0, model.ProductCount);
//            foreach (var item in model.Items)
//            {
//                Assert.Equal(item.CategoryId == "ChildCategory1" ? 2 : 0, item.Count);
//            }
//        }

//        // test: GetCategoriesAsync with a valid cateogryId (cached) returns an appropriate CategoryViewModel
//        [Fact]
//        public void GetCategories_ValidCategoryId_Cached()
//        {
//            // setup
//            const string categoryId = "CategoryId";
//            const string categoryImage = "CategoryImage";
//            const string displayName = "DisplayName";
//            SetupCacheService();
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            var cvm = new CategoryViewModel
//            {
//                CategoryId = categoryId,
//                DisplayName = displayName,
//                Image = categoryImage,
//                ParentCategoryId = categoryId,
//            };
//            cvm.Items.Add(new CategoryViewModel
//            {
//                CategoryId = "ChildCategory1",
//            });
//            cvm.Items[0].Items.Add(new CategoryViewModel
//            {
//                CategoryId = "ChildCategory11"
//            });
//            cvm.Items[0].Items.Add(new CategoryViewModel
//            {
//                CategoryId = "ChildCategory12"
//            });
//            cvm.Items.Add(new CategoryViewModel
//            {
//                CategoryId = "ChildCategory2"
//            });
//            cvm.Items.Add(new CategoryViewModel
//            {
//                CategoryId = "ChildCategory3"
//            });

//            _categoryCache.Setup(cc => cc.TryGet(cacheKey, out cvm)).Returns(true);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoriesAsync(categoryId, 2);

//            // sense
//            Assert.Equal(categoryId, model.CategoryId);
//            Assert.Equal(3, model.Count);
//            Assert.Equal(displayName, model.DisplayName);
//            Assert.Equal(categoryImage, model.Image);
//            Assert.Equal(categoryId, model.ParentCategoryId);
//            Assert.Equal(0, model.ProductCount);
//            foreach (var item in model.Items)
//            {
//                Assert.Equal(item.CategoryId == "ChildCategory1" ? 2 : 0, item.Count);
//            }
//        }

//        // test: GetCategoriesAsync with a valid cateogryId (not cached) and with a redirect catogory attribute set on one of its sub catagories
//        //      returns an appropriate CategoryViewModel for that sub cateogry with its category id substituted appropriately
//        [Fact]
//        public void GetCategories_SubstituteCategoryId_NotCached()
//        {
//            // setup
//            const string categoryId = "CategoryId";
//            const string substituteCategoryId = "SubstituteCategoryId";
//            var subCategories = new[]
//                {
//                    new ProductCategory
//                        {
//                            Attributes = new[] { new ExtendedAttribute { Name = CategoryViewModelBuilder.RedirectCategoryIdAttributeName, Value = substituteCategoryId } },
//                            CategoryId = "ChildCategory1",
//                        }
//                };
//            SetupCacheService();
//            CacheKey cacheKey;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            var productCategory = new ProductCategory
//            {
//                CategoryId = categoryId,
//                Attributes = null,
//                CategoryImage = "CategoryImage",
//                DisplayName = "DisplayName",
//                HasChildren = false,
//                SubCategories = subCategories,
//            };
//            _catalogAdapter.Setup(cs => cs.GetCategory(categoryId)).Returns(productCategory);
//            _catalogAdapter.Setup(cs => cs.GetCategories(categoryId, true)).Returns(subCategories);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().GetCategoriesAsync(categoryId);

//            // sense
//            Assert.Equal(1, model.Count);
//            Assert.Equal(substituteCategoryId, model.Items[0].CategoryId);
//        }

//        #endregion

//        #region SearchProductByCategoryAsync

//        // test: SearchProductByCategoryAsync passes the categoryId parameter correctly to the underlying GetCategoryAsync method
//        [Fact]
//        public void SearchProductByCategory_CategoryID_PassedCorrectlyToGetCategory()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            var catId = String.Empty;
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                           .Returns(new ProductCategory())
//                           .Callback<string>(cId => catId = cId);
//            _catalogAdapter.Setup(cs => cs.SearchByCategory(It.IsAny<string>(), null, null))
//                 .Returns((Products)null);

//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().SearchProductByCategoryAsync(categoryId);

//            // sense
//            Assert.Equal(categoryId, catId);
//        }

//        // test: SearchProductByCategoryAsync properly handles Category not found
//        [Fact]
//        public void SearchProductByCategory_CategoryID_HandlesNotFound()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "000000";
//            string catId = string.Empty;
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                           .Returns((ProductCategory)null)
//                           .Callback<string>(cId => catId = cId);
//            _catalogAdapter.Setup(cs => cs.SearchByCategory(It.IsAny<string>(), null, null))
//                 .Returns((Products)null);

//            // test
//            var model = Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().SearchProductByCategoryAsync(categoryId);

//            // sense
//            Assert.Equal(categoryId, catId);
//        }

//        // test: SearchProductByCategoryAsync passes the categoryId parameter correctly to the underlying category service's SearchByCategory method
//        [Fact]
//        public void SearchProductByCategory_CategoryID_PassedCorrectlyToSearchByCategory()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            var catId = String.Empty;
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                           .Returns(new ProductCategory());
//            _catalogAdapter.Setup(cs => cs.SearchByCategory(It.IsAny<string>(), null, null))
//                           .Returns((Products)null)
//                           .Callback<string, SearchOptions, FacetSearchField[]>((cId, so, fcts) => catId = cId);

//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().SearchProductByCategoryAsync(categoryId);

//            // sense
//            Assert.Equal(categoryId, catId);
//        }

//        // test: SearchProductByCategoryAsync returns an empty SearchResultViewModel if the categoryId parameter is invalid
//        [Fact]
//        public void SearchProductByCategory_InvalidCategoryId()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                           .Returns(new ProductCategory());
//            _catalogAdapter.Setup(cs => cs.SearchByCategory(It.IsAny<string>(), null, null))
//                 .Returns((Products)null);

//            // test
//            var model =
//               Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().SearchProductByCategoryAsync(categoryId);

//            // sense
//            Assert.Null(model.Title);
//            Assert.Null(model.Products);
//        }

//        // test: SearchProductByCategoryAsync returns a SearchResultViewModel with appropriate contents if the categoryId parameter is valid
//        [Fact]
//        public void SearchProductByCategory_ValidCategoryId()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            const string displayName = "DisplayName";
//            const string categoryImage = "SomeURL";
//            const int count = 1;
//            const int page = 1;
//            const int pageSize = 10;
//            const string productId = "Product1";
//            const string productDescription = "Product1 Description";
//            const int totalCount = 35;
//            const int totalPages = 4;

//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                .Returns(new ProductCategory
//                {
//                    CategoryId = categoryId,
//                    DisplayName = displayName,
//                    CategoryImage = categoryImage,
//                });
//            _catalogAdapter.Setup(cs => cs.SearchByCategory(It.IsAny<string>(), null, null))
//                 .Returns(new Products
//                 {
//                     Count = count,
//                     Page = page,
//                     PageSize = pageSize,
//                     Products = new[]
//                             {
//                                 new SearchProduct
//                                     {
//                                         Id = productId,
//                                         Description = productDescription,
//                                     }
//                             },
//                     TotalCount = totalCount,
//                     TotalPages = totalPages,
//                 });

//            // test
//            var model =
//               Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>().SearchProductByCategoryAsync(categoryId);
//            // sense
//            Assert.Equal(displayName, model.Title);
//            Assert.Equal(count, model.Products.Count);
//            Assert.Equal(page, model.Products.Page);
//            Assert.Equal(pageSize, model.Products.PageSize);
//            Assert.Equal(1, model.Products.Products.Length);
//            Assert.Equal(productId, model.Products.Products[0].Id);
//            Assert.Equal(productDescription, model.Products.Products[0].Description);
//            Assert.Equal(totalCount, model.Products.TotalCount);
//            Assert.Equal(totalPages, model.Products.TotalPages);
//        }

//        // test: SearchProductByCategoryAsync passes facets to catalog service
//        [Fact]
//        public void SearchProductByCategory_Facets_PassedToSearchByCategory()
//        {
//            // setup
//            SetupCacheService();
//            const string categoryId = "Category1";
//            FacetSearchField[] facets = null;
//            CacheKey cacheKey = null;
//            _cacheKeyResolver.Setup(ckr => ckr.TryGetCacheKey(out cacheKey, It.IsAny<string[]>())).Returns(true);
//            CategoryViewModel cvm;
//            _categoryCache.Setup(c => c.TryGet(cacheKey, out cvm)).Returns(false);
//            _catalogAdapter.Setup(cs => cs.GetCategory(It.IsAny<string>()))
//                           .Returns(new ProductCategory());
//            _catalogAdapter.Setup(cs => cs.SearchByCategory(It.IsAny<string>(), null, It.IsAny<FacetSearchField[]>()))
//                           .Returns((Products)null)
//                           .Callback<string, SearchOptions, FacetSearchField[]>((cId, so, fcts) => facets = fcts);

//            // test
//            Core.Services.Utils.DependencyResolver.Current.Get<ICategoryViewModelBuilder>()
//                .SearchProductByCategoryAsync(categoryId, null,
//                    new[]
//                    {
//                        new FacetSearchField
//                        {
//                            Constraints = new [] {new FacetConstraint {ConstraintQuery = "cq1"}},
//                            AttributeName = "an1"
//                        }
//                    });

//            // sense
//            Assert.NotNull(facets);
//            Assert.Equal(1, facets.Length);
//            Assert.Equal("an1", facets[0].AttributeName);
//            Assert.Equal(1, facets[0].Constraints.Length);
//            Assert.Equal("cq1", facets[0].Constraints[0].ConstraintQuery);
//        }

//        #endregion

//        #region private
//        private void SetupCacheService()
//        {
//            _cacheService.Setup(cs => cs.GetCache<Products>(It.IsAny<CacheConfig>())).Returns(_productCache.Object);
//            _cacheService.Setup(cs => cs.GetCache<CategoryViewModel>(It.IsAny<CacheConfig>())).Returns(_categoryCache.Object);
//            //_cacheService.Setup(cs => cs.GetCache<IEnumerable<Breadcrumb>>(It.IsAny<CacheConfig>())).Returns(_breadcrumbCache.Object);
//            //_cacheService.Setup(cs => cs.GetCache<IEnumerable<ProductMediaViewModel>>(It.IsAny<CacheConfig>())).Returns(_productMediaCache.Object);
//        }
//        #endregion
//    }
//}
