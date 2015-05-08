using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    public class CategoryPickerViewModel
    {
        public string SearchCriteria { get; set; }
        public CategoryViewModel Categories { get; set; }
        public string ControlId { get; set; }       // This will be the Parent Control ID, passed in
        public string CategoryId { get; set; }
    }
}
