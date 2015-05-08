namespace Jungo.Models.ShopperApi.Common
{
    public class AddProductToCartLink : ResourceLink
    {
        /// <summary>
        /// use this link instead of the base class's Uri because CartUri returns the cart object wherease Uri returns 201 with no payload
        /// </summary>
        public string CartUri { get; set; }
    }
}
