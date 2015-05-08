using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    public class ImagePickerViewModel
    {
        public ImagePickerViewModel(string controlId, string virtualPath)
		{
            ControlId = controlId;
            VirtualPath = virtualPath;
		}

        public string ControlId { get; set; }       // This will be the Parent Control ID, passed in
        public string VirtualPath { get; set; }
        public string SearchCriteria { get; set; }
        public string SortOrder { get; set; }
        public List<ImageViewModel> Images = new List<ImageViewModel>();
    }
}
