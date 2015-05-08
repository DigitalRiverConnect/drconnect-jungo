namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class OfferListViewModel
    {
        public string Title { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string[] SalesPitch { get; set; }
        public ProductOfferViewModel[] ProductOfferViewModels { get; set; }
    }
}
