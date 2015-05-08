using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
	public class OfferViewModel
	{
        public IList<Product> Products = new List<Product>();
        
        public string DisplayName { get; set; }
		public string AltText { get; set; }
		public string ThumbnailImage { get; set; }
		public string Image { get; set; }
		public string TargetUrl { get; set; }
		public string CategoryId { get; set; }
		public string TagLine { get; set; }
		public string Text { get; set; } // Text
		public string LinkText { get; set; } // Link Text
        public string HtmlContent { get; set; }

        // GC offer id
		public string OfferId { get; set; }
	}

	public class OffersViewModel : PageViewModelBase
	{
		// name of this offer
		public string DisplayName { get; set; }

		// actual offers
        public IList<OfferViewModel> Items = new List<OfferViewModel>();

		// number of actual offers
		public int Count { get { return Items.Count; } }

		// used to pass key/value data
		public Dictionary<string, string> Values { get; set; }

		// get key/value data with default handling
		public string Value(string key, string def = "")
		{
			string val;
			if (Values != null && Values.TryGetValue(key, out val))
				return val ?? "";

            return def;
		}
	}
}
