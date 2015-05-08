using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Session;
using Jungo.Api;
using Moq;
using Xunit;
using DependencyResolver = Jungo.Infrastructure.DependencyResolver;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Parts
{
	public class MiniCartPartControllerTests : TestBase
	{
        private readonly Mock<ICatalogApi> _catalogApi = new Mock<ICatalogApi>();
        private readonly Mock<IOffersApi> _offersApi = new Mock<IOffersApi>();
        private readonly Mock<ICartApi> _cartApi = new Mock<ICartApi>();
        private readonly Mock<WebSession> _webSession = new Mock<WebSession>();

		public MiniCartPartControllerTests()
		{
			DependencyRegistrar
				.StandardDependencies()
                .WithFakeLogger()
                .With(_catalogApi.Object)
                .With(_offersApi.Object)
                .With(_cartApi.Object);
            WebSession.Current = _webSession.Object;
        }

		#region Index
		// test: Index() renders ShoppingCart/MiniCartPartIndex
		[Fact]
		public void Index_Render()
		{
			// setup
            var ctrl =DependencyResolver.Current.Get<MiniCartPartController>();
            ctrl.SessionId = "1";

			// test
			var vres = (PartialViewResult)ctrl.Index();

			// sense
			Assert.Equal("Index", vres.ViewName);
		}

		#endregion

		#region GetMiniCart
		// test: GetMiniCart() renders ShoppingCart/MiniCartPart
		[Fact]
		public void GetMiniCart_Render()
		{
			// setup
            var ctrl =DependencyResolver.Current.Get<MiniCartPartController>();
            ctrl.SessionId = "1";
            //_cartApi.Setup(c => c.GetCartAsync()).Returns(new Task<Cart>(() => new Cart(), new CancellationToken()));

			// test
			var vres = (PartialViewResult)ctrl.GetMiniCart();

			// sense
			Assert.Equal("MiniCart", vres.ViewName);
		}

		// test: GetMiniCart() sets the ViewBag.Quantity
		[Fact]
		public void GetMiniCart_ViewBag_Quantity()
		{
			// setup
            var ctrl =DependencyResolver.Current.Get<MiniCartPartController>();
            ctrl.SessionId = "1";

            // test
            ctrl.GetMiniCart();

			// sense
			Assert.NotNull(ctrl.ViewBag);
            Assert.NotNull(ctrl.ViewBag.Quantity);
            Assert.Equal(0, ctrl.ViewBag.Quantity);
        }

		#endregion
	}
}
