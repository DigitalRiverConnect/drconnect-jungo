using System;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    [Serializable]
    public class ProductMediaViewModel
    {
        public string Type { get; set; }
        public string LargeImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string Content { get; set; }
        public string AltText { get; set; }
    }
}
