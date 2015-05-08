using System;
using System.Linq;
using Jungo.Infrastructure.Config;
using Jungo.Models.ShopperApi.Catalog;
using Jungo.Models.ShopperApi.Common;

namespace Jungo.Infrastructure.Extensions
{
    public static class CustomAttributeExtensions
    {

        public static string ValueByName(this CustomAttributes attributes, string name)
        {
            if (attributes == null || attributes.Attribute == null)
                return null;

            var attr =
                attributes.Attribute.FirstOrDefault(
                    attribute =>
                    attribute.Name != null &&
                    attribute.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            return (attr != null) ? attr.Value : null;
        }

        public static T ValueByName<T>(this CustomAttributes attributes, string name, T @default) where T : IConvertible
        {
            T result;

            try
            {
                var attr = attributes.ValueByName(name);
                if (string.IsNullOrEmpty(attr)) return @default; // avoid exception which slows down execution

                result = (T)Convert.ChangeType(attr, typeof(T));
            }
            catch
            {
                result = @default;
            }

            return result;
        }
    }
}
