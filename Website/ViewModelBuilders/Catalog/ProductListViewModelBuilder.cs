using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Jungo.Api;

namespace ViewModelBuilders.Catalog
{
    public interface IProductListViewModelBuilder
    {
        ProductListViewModel GetProductListViewModel(ProductListPart part, ILinkGenerator linkGenerator, ICatalogApi catalogApi);
    }

    public class ProductListViewModelBuilder : IProductListViewModelBuilder
    {
        public ProductListViewModel GetProductListViewModel(ProductListPart part, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
        {
            var model = new ProductListViewModel {Title = part.Title};

            foreach (var product in part.Products)
            {
                var itemModel = new ProductListItemViewModel
                {
                    Url = product.TargetUrl,
                    Title = product.Title,
                    Text = product.Text
                };
                long pid;
                if (long.TryParse(product.Product, out pid))
                {
                    itemModel.Product = catalogApi.GetProductAsync(catalogApi.GetProductUri(pid)).Result;
                    if (string.IsNullOrEmpty(itemModel.Url))
                        itemModel.Url = linkGenerator.GenerateProductLink(pid);
                    if (string.IsNullOrEmpty(itemModel.Title))
                        itemModel.Title = itemModel.Product.DisplayName;
                    if (string.IsNullOrEmpty(itemModel.Text))
                        itemModel.Text = itemModel.Product.ShortDescription;
                }
                model.Products.Add(itemModel);
            }

            return model;
        }
    }
}
