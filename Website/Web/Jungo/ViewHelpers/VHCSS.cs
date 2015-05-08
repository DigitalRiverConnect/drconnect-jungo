using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Jungo.Infrastructure.Config;
using Jungo.Infrastructure.Extensions;
using Jungo.Models.ShopperApi.Common;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.ViewHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public class VHCSS
    {
        /// <summary>
        /// When it's required to generate the style at runtime and cannot be easily accomplished through using standard CSS files
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CreateAttribute(string name, string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? name + ":" + value + ";" : "";
        }

        /// <summary>
        /// Because Product.ThumbnailImage and Product.ProductImage were designed for sites with simple image requirements (thumbnail and detail),
        /// we use this function to pull out images from the product attributes when multiple image sizes are needed. For the demo store, we have at least
        /// three sizes to choose from.
        /// </summary>
        /// <param name="product">The product that we would like to retrieve the image for.</param>
        /// <param name="attributeName">The product attribute that holds the image we would like to use.</param>
        /// <param name="fallbackUrl">A fallback image URL to use in case the image doesn't exist.</param>
        /// <returns>The URI of the image.</returns>
        public static string GetImageFromAttribute(Product product, string attributeName, string fallbackUrl)
        {
            var imgurl = product.CustomAttributes.ValueByName(attributeName) ?? fallbackUrl;

            if (!imgurl.StartsWith("http"))
            {
                imgurl = ConfigLoader.Get<ExternalWebLinkConfig>().ProductImageUrl + imgurl;
            }
            
            return imgurl;
        }
    }
}