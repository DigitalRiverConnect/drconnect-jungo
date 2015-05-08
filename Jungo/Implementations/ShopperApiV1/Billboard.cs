using System;
using System.Threading.Tasks;
using Jungo.Api;
using Jungo.Implementations.ShopperApiV1.Config;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Config;
using Jungo.Models.ShopperApi.Common;

namespace Jungo.Implementations.ShopperApiV1
{
    internal class Billboard
    {
        public V1Billboard RawBillboard { get; set; }
        public ShopperBillboard Shopper { get { return RawBillboard.Shoppers.Shopper; } }
        public string Token { get { return RawBillboard.ResourceAccess.Token.Uri; } }
        public string Categories { get { return Shopper.Categories.Uri; } }
        public string Products { get { return Shopper.Products.Uri; } }
        public string Offers { get { return Shopper.Offers.Uri; } }
        //public string PointOfPromotions { get { return Templates.ConstructPopsUri.Construct(Offers); } }
        public string PointOfPromotions { get { return Shopper.PointOfPromotions.Uri; } }
        public string Cart { get { return Shopper.Carts.Cart.Uri; } }
        public string WebCheckout { get { return Shopper.Carts.Cart.WebCheckout.Uri; } }

        private static ShopperApiUriTemplatesConfig _templatesConfig;
        public static ShopperApiUriTemplatesConfig Templates {
            get { return _templatesConfig ?? (_templatesConfig = ConfigLoader.Get<ShopperApiUriTemplatesConfig>()); }
        }

        private Billboard()
        {
        }

        private static Billboard _current;

        public static string GetSessionTokenUri()
        {
            var uriConfig = ConfigLoader.Get<ShopperApiUriConfig>();
            return uriConfig.SessionTokenUri;
        }

        public static async Task<ResourceAccessBillboard> GetResourceAccessAsync(IClient client)
        {
            if (_current != null) return _current.RawBillboard.ResourceAccess;
            var uriConfig = ConfigLoader.Get<ShopperApiUriConfig>();
            // make sure we do this as unauthorized, otherwise we might suffer from attempting to use an expired token and get in an endless loop
            var saveBearerToken = client.BearerToken;
            try
            {
                var res =
                    (await client.GetCacheableAsync<ShopperApiBillboard>(ResolveTemplate(uriConfig.BillboardUri,
                        Templates.ApiKeyQuery, new {apiKey = client.ApiKey})).ConfigureAwait(false)).V1.ResourceAccess;
                client.BearerToken = saveBearerToken;
                return res;
            }
            catch (Exception)
            {
                client.BearerToken = saveBearerToken;
                throw;
            }
        }

        public static async Task<Billboard> GetAsync(IClient client)
        {
            if (_current != null) return _current;
            var uriConfig = ConfigLoader.Get<ShopperApiUriConfig>();
            _current = new Billboard
            {
                RawBillboard = (await client.GetCacheableAsync<ShopperApiBillboard>(uriConfig.BillboardUri).ConfigureAwait(false)).V1
            };
            return _current;
        }

        public static string ResolveExpand(string uri, string expand)
        {
            return ResolveTemplate(uri, Templates.ExpandQuery, new { expand });
        }

        public static string ResolveExpandAll(string uri)
        {
            return ResolveExpand(uri, Templates.ExpandAllQueryValue);
        }

        public static string ResolvePagingOptions(string uri, PagingOptions options)
        {
            return (options == null) ? 
                ResolveExpandAll(uri) :
                ResolveTemplate(uri, Templates.PagingOptionsQuery, new { pageNumber = options.Page, pageSize = options.PageSize, sort = options.Sort });
        }

        public static string ResolveTemplate(string uri, string templatedQuery, object queryParams)
        {
            var tmplt = new UriTemplate(uri + templatedQuery);
            foreach (var substitution in queryParams.GetType().GetProperties())
            {
                var value = substitution.GetValue(queryParams, null);
                var substituionValue = value == null ? null : value.ToString();
                tmplt.SetParameter(substitution.Name, substituionValue);
            }
            return tmplt.Resolve();
        }
    }

    internal class ShopperApiBillboard
    {
        public V1Billboard V1 { get; set; }
    }

    internal class V1Billboard
    {
        public ResourceUri Site { get; set; }
        public ResourceAccessBillboard ResourceAccess { get; set; }
        public ShoppersBillboard Shoppers { get; set; }
    }

    internal class ResourceAccessBillboard
    {
        public ResourceUri Token { get; set; }
        public string Relation { get; set; }
        public ResourceUri Authorize { get; set; }
    }

    internal class ShoppersBillboard : ResourceLink
    {
        public ShopperBillboard Shopper { get; set; }
    }

    internal class ShopperBillboard : ResourceLink
    {
        public PurchasePlansBillboard PurchasePlans { get; set; }
        public ResourceLink Offers { get; set; }
        public ResourceLink PointOfPromotions { get; set; }
        public ResourceLink Categories { get; set; }
        public ResourceLink Products { get; set; }
        public CartsBillboard Carts { get; set; }
    }

    internal class PurchasePlansBillboard
    {
        public ResourceLink Search { get; set; }
        public ResourceLink Authorize { get; set; }
    }

    internal class CartsBillboard
    {
        public CartBillboard Cart { get; set; }
    }

    internal class CartBillboard : ResourceLink
    {
        public ResourceLink LineItems { get; set; }
        public ResourceLink BillingAddress { get; set; }
        public ResourceLink ShippingAddress { get; set; }
        public ResourceLink ShippingOptions { get; set; }
        public ResourceLink ApplyShippingOption { get; set; }
        public ResourceLink WebCheckout { get; set; }
    }
}
