using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jungo.Infrastructure.Config.Models;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Cart
{
    public class BuyViewModel
    {
        public SiteInfo SiteInfo { get; set; }
        public Uri FormActionUri { get; set; }
        public long ProductId { get; set; }
    }
}
