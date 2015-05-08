using System;
using System.Globalization;
using System.Linq;
using Jungo.Infrastructure;
using Jungo.Infrastructure.Config;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    [Serializable]
    public class CategoryRedirectConfig
    {
        public CategoryRedirect[] CategoryRedirects { get; set; }

        /// <summary>
        /// get configured redirection for a category id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>null if no redirect</returns>
        public static CategoryRedirect GetCategoryRedirect(long categoryId)
        {
            var catRedConfig = ConfigLoader.Get<CategoryRedirectConfig>();
            return catRedConfig != null
                ? catRedConfig.CategoryRedirects.FirstOrDefault(c => c.CategoryId == categoryId.ToString(CultureInfo.InvariantCulture))
                : null;
        }

        protected CategoryRedirect InternalGetCategoryRedirect(string categoryId)
        {
            return CategoryRedirects.FirstOrDefault(c => c.CategoryId == categoryId);
        }

    }

    public enum CategoryRedirectToWhat
    {
        CategoryId,
        ProductId,
        Path
    }

    [Serializable]
    public class CategoryRedirect
    {
        public string CategoryId { get; set; }
        public CategoryRedirectToWhat ToWhat { get; set; }
        public string Value { get; set; }
    }
}
