using System.Collections.Generic;
using System.Threading.Tasks;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace Jungo.Api
{
    public interface ICatalogApi
    {
        /// <summary>
        /// Get top-level categories (todo: recursive)
        /// </summary>
        /// <returns></returns>
        Task<Categories> GetCategoriesAsync();

        /// <summary>
        /// Get category tree from given category down
        /// </summary>
        /// <param name="categoryUri"></param>
        /// <returns></returns>
        Task<Category> GetCategoryAsync(ResourceUri categoryUri);

        /// <summary>
        /// Get products for a single category
        /// </summary>
        /// <param name="category">
        /// category for which products are to be returned;
        /// the category should have been retrieved with GetCategory() or GetCategories() in order for it to contain appropriate hypermedia to get the associated products</param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<Products> GetProductsForCategoryAsync(Category category, PagingOptions options = null);

        /// <summary>
        /// product keyword search
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<ProductsWithRanking> GetProductsByKeywordAsync(string keyword, PagingOptions options = null);

        /// <summary>
        /// Get a product
        /// </summary>
        /// <param name="productUri">
        /// </param>
        /// <returns></returns>
        Task<Product> GetProductAsync(ResourceUri productUri);

        /// <summary>
        /// Get multiple products at once
        /// </summary>
        /// <param name="productUris"></param>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetProductsAsync(IEnumerable<ResourceUri> productUris);

        /// <summary>
        /// Get multiple products at once
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetProductsAsync(IEnumerable<long> productIds);

        /// <summary>
        /// Manuevers to a parent product if necessary.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>If product is a base, returns its variations.
        ///  If a variation, returns its parent's variations.
        ///  If directly orderable, returns it.
        ///  If productId no good, returns null (not empty list).</returns>
        Task<IEnumerable<Product>> GetOrderableProductsAsync(long productId);

        /// <summary>
        /// Get baby product definitions for all products in the stinking catalog
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProductBrief>> GetProductBriefsAsync();

        /// <summary>
        /// Get inventory status for a list of uris
        /// </summary>
        /// <param name="inventoryStatusUris"></param>
        /// <returns></returns>
        Task<IEnumerable<InventoryStatus>> GetInventoryStatusesAsync(IEnumerable<ResourceUri> inventoryStatusUris);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        ResourceUri GetProductUri(long productId);

         /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        ResourceUri GetCategoryUri(long productId);
    }
}
