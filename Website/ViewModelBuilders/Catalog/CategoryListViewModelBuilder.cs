using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;

namespace ViewModelBuilders.Catalog
{
    public interface ICategoryListViewModelBuilder
    {
        CategoryListViewModel GetCategoryListViewModel(CategoryListPart part, ILinkGenerator linkGenerator, ICatalogApi catalogApi);
    }

    public class CategoryListViewModelBuilder : ICategoryListViewModelBuilder
    {
        public CategoryListViewModel GetCategoryListViewModel(CategoryListPart part, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
        {
            var model = new CategoryListViewModel { Title = part.Title };

            foreach (var category in part.Categories)
            {
                var itemModel = new CategoryListItemViewModel
                {
                    Url = category.TargetUrl,
                    Title = category.Title,
                    Text = category.Text
                };
                long cid;
                if (long.TryParse(category.Category, out cid))
                {
                    itemModel.Category = catalogApi.GetCategoryAsync(catalogApi.GetCategoryUri(cid)).Result;
                    if (string.IsNullOrEmpty(itemModel.Url))
                        itemModel.Url = linkGenerator.GenerateCategoryLink(cid);
                    if (string.IsNullOrEmpty(itemModel.Title))
                        itemModel.Title = itemModel.Category.DisplayName;
                    if (string.IsNullOrEmpty(itemModel.Text))
                        itemModel.Text = itemModel.Category.ShortDescription;
                }
                model.Categories.Add(itemModel);
            }

            return model;
        }
    }
}
