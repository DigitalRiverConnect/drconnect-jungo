namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    public class ProductPickerViewModel
    {
        public string SearchCriteria { get; set; }
        public string ControlId { get; set; }       // This will be the Parent Control ID, passed in
        public string Mode { get; set; }
        public string SelectedItems { get; set; }
    }
}
