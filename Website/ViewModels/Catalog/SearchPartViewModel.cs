namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class SearchPartViewModel
    {
        public string SiteId { get; set; }
        public string CultureCode { get; set; }
        public string ProductUrlRoot { get; set; }
        public string GcCultureCode { get; set; }
        public string AutocompleteCategoryId { get; set; }
        public int MaxResults { get; set; }
        public bool ShowProductImages { get; set; }
    }
}
