using System;

namespace Jungo.Implementations.ShopperApiV1.Config
{
    [Serializable]
    public class ShopperApiUriConfig
    {
        public string BillboardUri { get; set; }
        public string SessionTokenUri { get; set; }
    }
}
