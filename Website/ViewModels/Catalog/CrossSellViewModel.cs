using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class CrossSellViewModel : PageViewModelBase
    {
        public CrossSellViewModel()
        {
            Offers = new List<CrossSellOfferViewModel>();
        }

        public string Title { get; set; }
        public string Name { get; set; }
        public List<CrossSellOfferViewModel> Offers { get; set; }
        public string PromotionId { get; set; }
        public string EmptyCartPromotionId { get; set; }
        public int MaxNumberOfProducts { get; set; }
    }
}
