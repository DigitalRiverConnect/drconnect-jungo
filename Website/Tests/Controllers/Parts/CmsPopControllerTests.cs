//using System;
//using System.IO;
//using System.Security.Principal;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;
//using DigitalRiver.CloudLink.Commerce.Api.Adapters;
//using DigitalRiver.CloudLink.Commerce.Api.Catalog;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Parts;
//using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
//using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.PartsAdapters;
//using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
//using Moq;
//using N2;
//using N2.Collections;
//using N2.Security;
//using Xunit;

//namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Controllers.Parts
//{
//    public class CmsPopControllerTests : TestBase
//    {
//        private readonly Mock<ILinkGenerator> _linkGenerator = new Mock<ILinkGenerator>();
//        private readonly Mock<ICatalogAdapter> _catalogAdapter = new Mock<ICatalogAdapter>();
//        private readonly HttpContextTestData _httpContextTestData = new HttpContextTestData();
//        private readonly Mock<ISecurityManager> _securityManager = new Mock<ISecurityManager>();

//        public CmsPopControllerTests()
//        {
//            DependencyRegistrar
//                .StandardDependencies()
//                .WithFakeLogger()
//                .With(_linkGenerator.Object)
//                .With(_httpContextTestData)
//                .With(_catalogAdapter.Object);
//            // to satisfy ContentItem.GetChildren:
//            _securityManager.Setup(s => s.IsAuthorized(It.IsAny<ContentItem>(), It.IsAny<IPrincipal>())).Returns(true);
//            AccessFilter.CurrentSecurityManager = () => _securityManager.Object;
//        }

//        [Fact]
//        public void Index_BasicRender()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };

//            SetupFindView(true);
//            var helper = CreateHtmlHelper(new ViewDataDictionary());
            
//            var res = ctrl.PrepareRenderInfo(helper, currentItem);

//            Assert.NotNull(res);
//            Assert.Equal("PopTemplates/" + currentItem.ViewTemplate, res.Path);
//            Assert.NotNull(res.Model as OffersViewModel);
//            Assert.Equal(currentItem.Title, ((OffersViewModel)res.Model).DisplayName);
//            var compId = string.Empty;
//            try
//            {
//                compId = helper.ViewBag.ComponentID;
//            }
//            catch (Exception)
//            {
//                Assert.False(false, "ViewBag missing ComponentID");
//            }
//            Assert.Equal("item_5", compId);
//        }

//        [Fact]
//        public void Index_TemplateNotFound()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = null,
//            };

//            SetupFindView(false);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var res = ctrl.PrepareRenderInfo(helper, currentItem);

//            Assert.NotNull(res);
//            Assert.Equal("PopTemplates/NotFound", res.Path);
//            Assert.NotNull(res.Model as string);
//            Assert.Equal(currentItem.ViewTemplate, (string)res.Model);
//        }

//        [Fact]
//        public void Index_Hack_CmsTemplate_ReturnsCurrentItem()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "CmsHack",
//            };
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var res = ctrl.PrepareRenderInfo(helper, currentItem);

//            Assert.NotNull(res);
//            Assert.Equal("PopTemplates/" + currentItem.ViewTemplate, res.Path);
//            Assert.NotNull(res.Model as CmsPopPart);
//            Assert.Equal(currentItem.ViewTemplate, ((CmsPopPart)res.Model).ViewTemplate);
//        }

//        [Fact]
//        public void Index_OfferProps()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var promo = new PromotionOffer
//            {
//                TargetUrl = "myBasePromoLink",
//                Title = "myTitle",
//                ZoneName = "Offers",
//                TagLine = "myTagLine",
//                Text = "myText",
//                LinkText = "myLinkText",
//                Image = "myImage"
//            };
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };

//            currentItem.Children.Add(promo);
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = (ctrl.PrepareRenderInfo(helper, currentItem)).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(1, mod.Items.Count);
//            var offer = mod.Items[0];
//            Assert.Equal(promo.Title, offer.DisplayName);
//            Assert.Equal(promo.TargetUrl, offer.TargetUrl);
//            Assert.Equal(promo.TagLine, offer.TagLine);
//            Assert.Equal(promo.Text, offer.Text);
//            Assert.Equal(promo.LinkText, offer.LinkText);
//            Assert.Equal(promo.Image, offer.Image);
//        }

//        [Fact]
//        public void Index_CategoryPromo()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };
//            currentItem.Children.Add(new CategoryPromotionOffer { Category = "myCategory", Title = "myTitle", ZoneName = "Offers" });
//            _linkGenerator.Setup(l => l.GenerateCategoryLink("myCategory", It.IsAny<bool?>())).Returns("myCategoryLink");
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = ctrl.PrepareRenderInfo(helper, currentItem).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(1, mod.Items.Count);
//            Assert.Equal("myTitle", mod.Items[0].DisplayName);
//            Assert.Equal("myCategoryLink", mod.Items[0].TargetUrl);
//        }

//        [Fact]
//        public void Index_ProductPromo()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };
//            currentItem.Children.Add(new ProductPromotionOffer { Product = "myProduct", Title = "myTitle", ZoneName = "Offers" });
//            _linkGenerator.Setup(l => l.GenerateProductLink("myProduct")).Returns("myProductLink");
//            var myProd = new Product {Description = "myProductDescription"};
//            _catalogAdapter.Setup(c => c.TryGetProduct("myProduct", out myProd))
//                .Returns(true);
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = ctrl.PrepareRenderInfo(helper, currentItem).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(1, mod.Items.Count);
//            Assert.Equal("myTitle", mod.Items[0].DisplayName);
//            Assert.Equal("myProductLink", mod.Items[0].TargetUrl);
//            Assert.Equal(1, mod.Items[0].Products.Count);
//            Assert.Equal("myProductDescription", mod.Items[0].Products[0].Description);
//        }

//        [Fact]
//        public void Index_HelpDeskPromo_Detail()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };
//            currentItem.Children.Add(new HelpDeskPromotionOffer
//            {
//                Product = "myProduct",
//                Title = "myTitle",
//                ZoneName = "Offers",
//                ShowProductDetail = true
//            });
//            _linkGenerator.Setup(l => l.GenerateHelpDeskLink("myProduct")).Returns("myHelpDeskProductLink");
//            var myProd = new Product {Description = "myProductDescription"};
//            _catalogAdapter.Setup(c => c.TryGetProduct("myProduct", out myProd))
//                .Returns(true);
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = ctrl.PrepareRenderInfo(helper, currentItem).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(1, mod.Items.Count);
//            Assert.Equal("myTitle", mod.Items[0].DisplayName);
//            Assert.Equal("myHelpDeskProductLink", mod.Items[0].TargetUrl);
//            Assert.Equal(1, mod.Items[0].Products.Count);
//            Assert.Equal("myProductDescription", mod.Items[0].Products[0].Description);
//        }

//        [Fact]
//        public void Index_HelpDeskPromo_NoDetail()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };
//            currentItem.Children.Add(new HelpDeskPromotionOffer
//            {
//                Product = "myProduct",
//                Title = "myTitle",
//                ZoneName = "Offers",
//                ShowProductDetail = false
//            });
//            _linkGenerator.Setup(l => l.GenerateHelpDeskLink("myProduct")).Returns("myHelpDeskProductLink");
//            _catalogAdapter.Setup(c => c.GetProduct("myProduct"))
//                .Returns(new Product { Description = "myProductDescription" });
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = ctrl.PrepareRenderInfo(helper, currentItem).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(1, mod.Items.Count);
//            Assert.Equal("myTitle", mod.Items[0].DisplayName);
//            Assert.Equal("myHelpDeskProductLink", mod.Items[0].TargetUrl);
//            Assert.Equal(0, mod.Items[0].Products.Count);
//        }

//        [Fact]
//        public void Index_BasePromo()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };
//            currentItem.Children.Add(new PromotionOffer { TargetUrl = "myBasePromoLink", Title = "myTitle", ZoneName = "Offers" });
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = ctrl.PrepareRenderInfo(helper, currentItem).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(1, mod.Items.Count);
//            Assert.Equal("myTitle", mod.Items[0].DisplayName);
//            Assert.Equal("myBasePromoLink", mod.Items[0].TargetUrl);
//        }

//        [Fact]
//        public void Index_MultiplePromos()
//        {
//            var ctrl = Core.Services.Utils.DependencyResolver.Current.Get<CmsPopAdapter>();
//            var currentItem = new CmsPopPart
//            {
//                ViewTemplate = "myTemplate",
//                Title = "myTitle",
//                ID = 5
//            };
//            currentItem.Children.Add(new PromotionOffer { TargetUrl = "myBasePromoLink1", Title = "myTitle1", ZoneName = "Offers" });
//            currentItem.Children.Add(new PromotionOffer { TargetUrl = "myBasePromoLink2", Title = "myTitle2", ZoneName = "Offers" });
//            SetupFindView(true);

//            var helper = CreateHtmlHelper(new ViewDataDictionary());
//            var mod = ctrl.PrepareRenderInfo(helper, currentItem).Model as OffersViewModel;

//            Assert.NotNull(mod);
//            Assert.Equal(2, mod.Items.Count);
//            Assert.Equal("myTitle1", mod.Items[0].DisplayName);
//            Assert.Equal("myBasePromoLink1", mod.Items[0].TargetUrl);
//            Assert.Equal("myTitle2", mod.Items[1].DisplayName);
//            Assert.Equal("myBasePromoLink2", mod.Items[1].TargetUrl);
//        }

//        private static void SetupFindView(bool bFind)
//        {
//            var engine = new Mock<IViewEngine>();
//            engine.Setup(
//                e => e.FindView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                .Returns(bFind
//                    ? new ViewEngineResult(new Mock<IView>().Object, engine.Object)
//                    : new ViewEngineResult(new[] {"loc1"}));
//            ViewEngines.Engines.Clear();
//            ViewEngines.Engines.Add(engine.Object);
//        }

//        public static HtmlHelper CreateHtmlHelper(ViewDataDictionary vd)
//        {
//            var mockViewContext = new Mock<ViewContext>(
//              new ControllerContext(
//                new Mock<HttpContextBase>().Object,
//                new RouteData(),
//                new Mock<ControllerBase>().Object),
//              new Mock<IView>().Object,
//              vd,
//              new TempDataDictionary(),
//              new StringWriter());

//            var mockViewDataContainer = new Mock<IViewDataContainer>();
//            mockViewDataContainer.Setup(v => v.ViewData).Returns(vd);

//            return new HtmlHelper(mockViewContext.Object, mockViewDataContainer.Object);
//        }
//    }

//    //public class MyCmsPopController : CmsPopController
//    //{
//    //    public MyCmsPopController(ILinkGenerator linkGenerator, ICmsCatalogAdapter cmsCatalogAdapter, HttpContextTestData myTestData)
//    //        : base(linkGenerator, cmsCatalogAdapter)
//    //    {
//    //        Content = new ControllerContentHelper(null, () => new PathData(MyCurrentPage, MyCurrentItem));
//    //        this.SetMockControllerContext(myTestData);
//    //    }
//    //    public CmsPopPart MyCurrentItem = new CmsPopPart { ViewTemplate = "t" };
//    //    public ProductPage MyCurrentPage = new ProductPage();
//    //}
//}
