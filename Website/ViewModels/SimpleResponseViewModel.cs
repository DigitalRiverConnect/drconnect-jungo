using System.Collections.Generic;
using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Attributes;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels
{
    public class SimpleResponseViewModel
    {
        [JsonProperty(Name = "success")]
        public bool Success { get; set; }

        [JsonProperty(Name = "errorMessages")]
        public IEnumerable<string> ErrorMessages { get; set; }

        public object Model { get; set; }
    }
}
