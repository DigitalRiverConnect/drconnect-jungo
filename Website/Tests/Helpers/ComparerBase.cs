using System.Collections.Generic;
using System.Linq;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Helpers
{
    abstract class ComparerBase
    {
        public static bool AreCollectionItemsEqual<T>(IEnumerable<T> x, IEnumerable<T> y, IEqualityComparer<T> comparer)
        {
            var a = x.ToArray();
            var b = y.ToArray();

            if (a.Length == b.Length)
            {
                for (var i = 1; i < a.Length; i++)
                {
                    if (!comparer.Equals(a[i], b[i]))
                        return false;
                }
            }

            return true;
        }
    }
}
