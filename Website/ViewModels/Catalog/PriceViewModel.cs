
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class PriceViewModel : PageViewModelBase
    {
        public PriceViewModel()
        {
            Pricing = new Pricing();
            MinPrice = new Pricing();
            MaxPrice = new Pricing();
            Initialize();
        }

        public PriceViewModel(Pricing pricing, Pricing minPrice, Pricing maxPrice, bool showLowestPrice = false)
        {
            Pricing = pricing;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            ShowLowestPrice = showLowestPrice;
            Initialize();
        }

        public Pricing Pricing { get; private set; }

        public Pricing MinPrice { get; private set; }

        public Pricing MaxPrice { get; private set; }

        public bool ShowLowestPrice { get; private set; }

        public void Initialize()
        {

        }
    }
}
