using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using Jungo.Api;
using Jungo.Infrastructure.Logger;
using N2.Web;
using ViewModelBuilders.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
    [Controls(typeof(ProductListPart))]
    public class ProductListController : ContentControllerBase<ProductListPart>
    {
        private readonly IProductListViewModelBuilder _modelBuilder;
        public ProductListController(IProductListViewModelBuilder modelBuilder, IRequestLogger logger, ILinkGenerator linkGenerator, ICatalogApi catalogApi)
            : base(logger, linkGenerator, catalogApi)
        {
            _modelBuilder = modelBuilder;
        }

        public override ActionResult Index()
        {
            return PartialView(_modelBuilder.GetProductListViewModel(CurrentItem, LinkGenerator, CatalogApi));
        }

    }
}
