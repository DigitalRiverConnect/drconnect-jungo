using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(CategoryListPart))]
    public class CategoryListController : ContentControllerBase<CategoryListPart>
    {
        private readonly ICategoryListViewModelBuilder _modelBuilder;
        public CategoryListController(ICategoryListViewModelBuilder modelBuilder, IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            _modelBuilder = modelBuilder;
        }

        public override ActionResult Index()
        {
            return PartialView(_modelBuilder.GetCategoryListViewModel(CurrentItem, LinkGenerator, CatalogApi));
        }

    }
}
