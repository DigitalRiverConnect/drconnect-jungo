using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Jungo.Api;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Logger;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;
using Omu.ValueInjecter;
using Product = Jungo.Models.ShopperApi.Catalog.Product;

namespace Jungo.Implementations.ShopperApiV1
{
    public class CatalogApi : ICatalogApi
    {
        #region ICatalogApi Members

        public async Task<Categories> GetCategoriesAsync()
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    //todo: recursive (and let's do it in parallel, please)
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var response =
                        await
                            Client.GetCacheableAsync<Category>(Billboard.ResolveExpandAll(billboard.Categories))
                                .ConfigureAwait(false);
                    return response.Categories;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Category> GetCategoryAsync(ResourceUri categoryUri)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var uri = Billboard.ResolveExpandAll(categoryUri.Uri);
                    var categoriesResponse =
                        await Client.GetCacheableAsync<CategoriesResponse>(uri).ConfigureAwait(false);
                    return categoriesResponse == null ? null : categoriesResponse.Category;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<Products> GetProductsForCategoryAsync(Category category, PagingOptions options)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var uri = Billboard.ResolvePagingOptions(category.Products.Uri, options);
                    var prodResponse = await Client.GetCacheableAsync<ProductsResponse>(uri).ConfigureAwait(false);
                    if (prodResponse == null) return null;
                    var catProds = prodResponse.Products;
                    catProds.Product = (await GetProductsAsync(catProds.Product).ConfigureAwait(false)).ToArray();
                    return catProds;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<ProductsWithRanking> GetProductsByKeywordAsync(string keyword, PagingOptions options)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                ProductsWithRanking kwProds;
                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    // nudge, nudge: gives us back a baby srch result, not the full ProductWithRanking
                    var kwUri = Billboard.ResolveTemplate(billboard.Products, Billboard.Templates.ProductsKeywordQuery,
                        new {keyword, pageNumber = options.Page, pageSize = options.PageSize, sort = options.Sort});
                    var response =
                        await Client.GetCacheableAsync<ProductsWithRankingResponse>(kwUri).ConfigureAwait(false);
                    if (response == null || response.Products == null || response.Products.Product == null) return new ProductsWithRanking {Product = new ProductWithRanking[0]};
                    kwProds = response.Products;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }

                // fire up parallel calls to get papa products
                var prods = new Task<Product>[kwProds.Product.Length];
                for (var idx = 0; idx < kwProds.Product.Length; idx++)
                    prods[idx] = GetProductAsync(kwProds.Product[idx]);
                // wait for all
                // put papa prods into keywords response
                var exceptions = new List<Exception>();
                for (var idx = 0; idx < prods.Length; idx++)
                {
                    try
                    {
                        var response = await prods[idx].ConfigureAwait(false);
                        if (response != null)
                            kwProds.Product[idx].InjectFrom(response);
                    }
                    catch (Exception exc)
                    {
                        exceptions.Add(exc);
                    }
                }
                if (exceptions.Any())
                    throw log.LogException(new AggregateException(exceptions));
                return kwProds;
            }
        }

        public async Task<Product> GetProductAsync(ResourceUri productUri)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var uri = Billboard.ResolveExpandAll(productUri.Uri);
                    var productResponse = await Client.GetCacheableAsync<ProductResponse>(uri).ConfigureAwait(false);
                    if (productResponse == null) return null;

                    var prod = productResponse.Product;
                    await SupplyRecentInventoryStatusAsync(new[] {prod}).ConfigureAwait(false);
                    return prod;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(IEnumerable<ResourceUri> productUris)
        {
            // todo: consider parsing uris to get product id off the end and then call the other GetProducts signature with an array of longs
            // todo:   this will reduce the one round trip per product to just one for the whole mess
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                var uris = productUris.ToArray();
                var prodGet = new Task<Product>[uris.Length];
                var products = new Product[uris.Length];

                for (var idx = 0; idx < uris.Length; idx++)
                    prodGet[idx] = GetProductAsync(uris[idx]);

                var exceptions = new List<Exception>();
                // wait for all
                // put papa prods into response
                for (var idx = 0; idx < prodGet.Length; idx++)
                {
                    try
                    {
                        var product = await prodGet[idx].ConfigureAwait(false);
                        if (product != null)
                            products[idx] = product;
                    }
                    catch (Exception exc)
                    {
                        exceptions.Add(exc);
                    }
                }
                if (exceptions.Any())
                    throw log.LogException(new AggregateException(exceptions));
                return products.Where(p => p != null);
            }
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(IEnumerable<long> productIds)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var ids = productIds.ToArray();
                    if (!ids.Any())
                        return new Product[0];

                    var idsStr = ids[0].ToString(CultureInfo.InvariantCulture);
                    for (var idsIdx = 1; idsIdx < ids.Length; idsIdx++)
                        idsStr += "," + ids[idsIdx].ToString(CultureInfo.InvariantCulture);

                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var uri = Billboard.ResolveTemplate(billboard.Products, Billboard.Templates.MultipleProductQuery,
                        new {productIds = idsStr, expand = Billboard.Templates.ExpandAllQueryValue});

                    var prodsResp = await Client.GetCacheableAsync<ProductsResponse>(uri).ConfigureAwait(false);

                    if (prodsResp == null || prodsResp.Products == null || prodsResp.Products.Product == null)
                        return new Product[0];

                    await SupplyRecentInventoryStatusAsync(prodsResp.Products.Product).ConfigureAwait(false);

                    await SupplyRecentInventoryStatusAsync(prodsResp.Products.Product.Where(p => p.Variations != null).SelectMany(p => p.Variations.Product)).ConfigureAwait(false);

                    return prodsResp.Products.Product;
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<IEnumerable<Product>> GetOrderableProductsAsync(long productId)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var prod = await GetProductAsync(GetProductUri(productId)).ConfigureAwait(false);
                    if (prod != null && prod.ParentProduct != null) // a variation; get its parent
                        prod = await GetProductAsync(prod.ParentProduct).ConfigureAwait(false);
                    if (prod == null) return null;
                    if (!prod.BaseProduct || prod.Variations == null || prod.Variations.Product == null ||
                        prod.Variations.Product.Length == 0)
                        return new[] {prod}; // a directly-orderable product, not a base, doesn't have variations
                    return prod.Variations.Product; // a base product; return its variations
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<IEnumerable<ProductBrief>> GetProductBriefsAsync()
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                try
                {
                    var billboard = await Billboard.GetAsync(Client).ConfigureAwait(false);
                    var uri = Billboard.ResolveExpand(billboard.Products, Billboard.Templates.ExpandProductIdQueryValue);
                    var prodsResponse = await Client.GetCacheableAsync<ProductsResponse>(uri).ConfigureAwait(false);
                    if (prodsResponse == null || prodsResponse.Products == null ||
                        prodsResponse.Products.Product == null)
                        return new ProductBrief[0];
                    return prodsResponse.Products.Product.Select(p => new ProductBrief
                    {
                        Id = p.Id,
                        Uri = p.Uri,
                        Relation = p.Relation,
                        DisplayName = p.DisplayName,
                        ThumbnailImage = p.ThumbnailImage
                    });
                }
                catch (Exception exc)
                {
                    throw log.LogException(exc);
                }
            }
        }

        public async Task<IEnumerable<InventoryStatus>> GetInventoryStatusesAsync(IEnumerable<ResourceUri> inventoryStatusUris)
        {
            using (var log = RequestLogger.Current.BeginJungoLog(this))
            {
                var uris = inventoryStatusUris.Select(i => i.Uri).ToArray();
                if (!uris.Any())
                    return new InventoryStatus[0];

                // todo: shorter TTL on cached inventory status compared to product TTL

                // fire up parallel calls to get inventory
                var statResps = new Task<InventoryStatusResponse>[uris.Length];
                for (var idx = 0; idx < uris.Length; idx++)
                    statResps[idx] = Client.GetCacheableAsync<InventoryStatusResponse>(uris[idx]);
                // wait for all
                // put statuses into response
                var stats = new List<InventoryStatus>();
                var exceptions = new List<Exception>();
                foreach (var resp in statResps)
                {
                    try
                    {
                        var response = await resp.ConfigureAwait(false);
                        if (response != null)
                            stats.Add(response.InventoryStatus);
                    }
                    catch (Exception exc)
                    {
                        exceptions.Add(exc);
                    }
                }
                if (exceptions.Any())
                    throw log.LogException(new AggregateException(exceptions));
                return stats;
            }
        }

        public ResourceUri GetProductUri(long productId)
        {
            var billboard = Billboard.GetAsync(Client).Result;
            return new ResourceUri
            {
                Uri = Billboard.ResolveTemplate(billboard.Products, Billboard.Templates.IdSegment, new { id = productId })
            };
        }


        public ResourceUri GetCategoryUri(long categoryId)
        {
            var billboard = Billboard.GetAsync(Client).Result;
            return new ResourceUri
            {
                Uri = Billboard.ResolveTemplate(billboard.Categories, Billboard.Templates.IdSegment, new { id = categoryId })
            };
        }

        #endregion

        #region implementation

        internal async Task SupplyRecentInventoryStatusAsync(IEnumerable<Models.ShopperApi.Common.Product> products)
        {
            // get possibly more recent inventory status in case we got cached products
            var prodList = products.ToArray();
            var uris =
                prodList.Select(
                    p => (p.InventoryStatus == null || string.IsNullOrEmpty(p.InventoryStatus.Uri))
                        ? new ResourceUri {Uri = p.Uri + Billboard.Templates.InventoryStatusSegment}
                        : p.InventoryStatus).ToArray();
            var invStats = await GetInventoryStatusesAsync(uris).ConfigureAwait(false);
            // overlay new inventory status into product
            foreach (var invStat in invStats)
            {
                var prod = prodList.FirstOrDefault(
                    p => string.Compare(p.Uri, invStat.Product[0].Uri, StringComparison.OrdinalIgnoreCase) == 0);
                if (prod != null)
                    prod.InventoryStatus = invStat;
            }
        }

        // Do not make this property static, despite Resharper suggesting you can. To do so would create cross-request problems!!
        private IClient Client { get { return DependencyResolver.Current.Get<IClient>(); } }

        #endregion
    }
}
