namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    public class ContentPickerViewModel
    {
        public string SearchCriteria { get; set; }
        public string ControlId { get; set; }       // This will be the Parent Control ID, passed in
        public ContentItemViewModel StartPage { get; set; }
    }
}
