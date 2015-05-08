 //ExtendedAttribute
using System;
using System.Collections.Generic;
using System.Linq;
using Jungo.Models.ShopperApi.Common;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    [Serializable]
	public class CategoryViewModel
	{
		public CategoryViewModel()
		{
			Items = new List<CategoryViewModel>();
            Attributes = new CustomAttributes();
		}

		public IList<CategoryViewModel> Items { get; private set; }
		public long? ParentCategoryId { get; set; }
		public long CategoryId { get; set; }
		public string DisplayName { get; set; }
		public string Image { get; set; }
        public string Keywords { get; set; }
        public CustomAttributes Attributes { get; set; }
	}

    public class SortCategoryViewModel : PageViewModelBase
    {
        public SortCategoryViewModel()
		{
            Items = new List<Option>();
		}
        public IList<Option> Items { get; private set; }

        public string SelectedText
        {
            get { return Items.Where(i => i.Selected).Select(i => i.Name).FirstOrDefault(); }
        }
    }
}
