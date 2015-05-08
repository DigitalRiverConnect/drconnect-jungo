using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers
{
    public interface INimbusContentController
    {
        Dictionary<long, Product> Products { get; }
        List<long> BogusProductIds { get; }
    }
}