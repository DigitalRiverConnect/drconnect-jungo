namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    public class ContentItemViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public bool IsPage { get; set; }
        public string Url { get; set; }
        public ContentItemViewModel[] Children { get; set; }
    }
}