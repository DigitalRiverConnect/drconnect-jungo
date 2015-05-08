namespace Jungo.Models.ShopperApi.Catalog
{
    /// <summary>
    /// returned if keyword search done
    /// </summary>
    public class ProductWithRanking : Product
    {
        public int Ranking { get; set; }
        public int Score { get; set; }
    }
}
