using System;
using System.Threading.Tasks;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;
using Jungo.Infrastructure.Extensions;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace ViewModelBuilders.Catalog
{
    public interface ICategoryViewModelBuilder // for testing
    {
        Task<CategoryViewModel> GetCategoryAsync(long categoryId);
        Task<CategoryViewModel> GetCategoriesAsync(long? categoryId = null, int levels = 1);
        Task<CatalogPageViewModel> SearchProductByCategoryAsync(long categoryId, PagingOptions pagingOptions = null);
    }

    public class CategoryViewModelBuilder : ICategoryViewModelBuilder
    {
        public const string RedirectCategoryIdAttributeName = "RedirectCategoryId";
        private const int FirstPage = 1;
        private const int DefaultPageSize = 1;

        private readonly ICatalogApi _catalogApi;

        public CategoryViewModelBuilder(ICatalogApi catalogApi)
        {
            _catalogApi = catalogApi;
        }

        public async Task<CategoryViewModel> GetCategoryAsync(long categoryId)
        {
            var category = await _catalogApi.GetCategoryAsync(_catalogApi.GetCategoryUri(categoryId)).ConfigureAwait(false);
            return category == null ? null : CreateCategoryViewModel(category, categoryId);
        }

        public async Task<CategoryViewModel> GetCategoriesAsync(long? categoryId = null, int levels = 1)
        {
            CategoryViewModel result;

            if (categoryId.HasValue)
            {
                var category = await _catalogApi.GetCategoryAsync(_catalogApi.GetCategoryUri(categoryId.Value)).ConfigureAwait(false);
                if (category == null)
                {
                    result = null;
                }
                else
                {
                    var parent = CreateCategoryViewModel(category, category.Id);
                    await GenerateChildCategoryViewModels(parent, category.Categories, levels).ConfigureAwait(false);
                    result = parent;
                }
            }
            else
            {
                var categories = await _catalogApi.GetCategoriesAsync().ConfigureAwait(false);

                var categoryViewModel = new CategoryViewModel
                {
                    CategoryId = 0,
                    DisplayName = "Root",
                    Image = string.Empty
                };

                await GenerateChildCategoryViewModels(categoryViewModel, categories, levels).ConfigureAwait(false);

                result = categoryViewModel;
            }

            return result;
        }

        public async Task<CatalogPageViewModel> SearchProductByCategoryAsync(long categoryId, PagingOptions pagingOptions)
        {
            var category = await _catalogApi.GetCategoryAsync(_catalogApi.GetCategoryUri(categoryId)).ConfigureAwait(false);
            var categoryViewModel = CreateCategoryViewModel(category, categoryId);
            if (pagingOptions == null) pagingOptions = new PagingOptions();

            if (categoryViewModel == null)
            {

                var viewModel = new CatalogPageViewModel
                {
                    CategoryId = categoryId,
                    Products = new Products { Product = new Product[0] }
                };
                viewModel.SetPageTitle("Catalog");
                return viewModel;
            }

            Products searchResult;
            try
            {
                searchResult = await _catalogApi.GetProductsForCategoryAsync(category, pagingOptions).ConfigureAwait(false);
            }
            catch (Exception)
            {
                searchResult = new Products { Product = new Product[0] };
            }

            var viewMod = new CatalogPageViewModel
            {
                Title = categoryViewModel.DisplayName,
                CategoryId = categoryId,
                Products = searchResult,
                KeyWords = categoryViewModel.Keywords,
                TotalPages = searchResult.TotalResultPages,
                TotalResults = searchResult.TotalResults,
                PageSize = pagingOptions.PageSize ?? DefaultPageSize,
                CurrentPage = pagingOptions.Page ?? FirstPage,
                SeoTitleTag = categoryViewModel.Attributes.ValueByName("Custom Title"),
                SeoMetaDescription = categoryViewModel.Attributes.ValueByName("Meta Description"),
                SeoMetaKeywords = categoryViewModel.Attributes.ValueByName("Meta Keywords")
            };
            viewMod.SetPageTitle(categoryViewModel.DisplayName);

            return viewMod;
        }


        private CategoryViewModel CreateCategoryViewModel(Category category, long? parentCategoryId)
        {
            if (category == null) return null;

            return new CategoryViewModel
            {
                CategoryId = category.Id,
                DisplayName = category.DisplayName,
                Image = category.ThumbnailImage,
                ParentCategoryId = parentCategoryId,
                //Keywords = category.Keywords,
                Attributes = category.CustomAttributes
            };
        }

        private async Task GenerateChildCategoryViewModels(CategoryViewModel categoryViewModel,
            Categories categories, int levelsRemaining)
        {
            if (levelsRemaining <= 0 || categories == null || categories.Category == null) return;

            foreach (var item in categories.Category)
            {
                var currentCategory = await _catalogApi.GetCategoryAsync(item).ConfigureAwait(false);
                var itemViewModel = CreateCategoryViewModel(currentCategory, currentCategory.Id);
                categoryViewModel.Items.Add(itemViewModel);
                await GenerateChildCategoryViewModels(itemViewModel, currentCategory.Categories, levelsRemaining - 1).ConfigureAwait(false);
            }
        }
    }
}
