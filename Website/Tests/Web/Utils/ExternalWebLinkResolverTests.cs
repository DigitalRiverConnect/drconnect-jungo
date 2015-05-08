using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Xunit;
using Xunit.Extensions;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Web.Utils
{
    public class ExternalWebLinkResolverTests : TestBase
    {
        [Fact]
        public void UriWithoutProtocol()
        {
            var res = ExternalWebLinkResolver.UriWithoutProtocol("http://myserver.com/root/uploads/image.jpg");
            Assert.Equal("//myserver.com/root/uploads/image.jpg", res);
        }

        [Fact]
        public void UriWithoutProtocol2()
        {
            var res = ExternalWebLinkResolver.UriWithoutProtocol("https://myserver.com/root/uploads/image.jpg");
            Assert.Equal("//myserver.com/root/uploads/image.jpg", res);
        }
        
        [Fact]
        public void UriWithoutProtocol3()
        {
            var res = ExternalWebLinkResolver.UriWithoutProtocol("//myserver.com/root/uploads/image.jpg");
            Assert.Equal("//myserver.com/root/uploads/image.jpg", res);
        }

        [Fact]
        public void FormatPublicUrl_SupplyScheme()
        {
            var res = ExternalWebLinkResolver.FormatPublicUrl("https", "myserver.com/root", "~/uploads/image.jpg");
            Assert.Equal("https://myserver.com/root/uploads/image.jpg", res);
        }

        [Fact]
        public void FormatPublicUrl_UseConfiguredScheme()
        {
            var res = ExternalWebLinkResolver.FormatPublicUrl("http", "https://myserver.com/root", "~/uploads/image.jpg");
            Assert.Equal("https://myserver.com/root/uploads/image.jpg", res);
        }

        [Fact]
        public void FormatPublicUrl_UseConfiguredScheme2()
        {
            var res = ExternalWebLinkResolver.FormatPublicUrl("http", "//myserver.com/root", "~/uploads/image.jpg");
            Assert.Equal("http://myserver.com/root/uploads/image.jpg", res);
        }

        [Fact]
        public void FormatPublicUrl_WithoutScheme()
        {
            var res = ExternalWebLinkResolver.FormatPublicUrl(null, "https://myserver.com/root", "~/uploads/image.jpg");
            Assert.Equal("//myserver.com/root/uploads/image.jpg", res);
        }

        [Fact]
        public void FormatPublicUrl_WithoutScheme2()
        {
            var res = ExternalWebLinkResolver.FormatPublicUrl(null, "//myserver.com/root", "~/uploads/image.jpg");
            Assert.Equal("//myserver.com/root/uploads/image.jpg", res);
        }

        [Theory,
         InlineData("myserver.com/root", "uploads/image.jpg"),
         InlineData("myserver.com/root", "/uploads/image.jpg"),
         InlineData("myserver.com/root/", "uploads/image.jpg"),
         InlineData("myserver.com/root/", "/uploads/image.jpg")
        ]
        public void FormatPublicUrl_Whacks(string publicUrl, string imageUrl)
        {
            var res = ExternalWebLinkResolver.FormatPublicUrl("http", publicUrl, imageUrl);
            Assert.Equal("http://myserver.com/root/uploads/image.jpg", res);
        }

        [Theory,
         InlineData("uploads/image.jpg"),
         InlineData("/uploads/image.jpg"),
         InlineData("~/uploads/image.jpg")         
        ]
        public void ToPublicUrl_WithoutConfig(string imageUrl)
        {
            var res = ExternalWebLinkResolver.ToPublicUrl(imageUrl, null);
            Assert.Equal("/uploads/image.jpg", res);
        }

        //[Theory,
        //    InlineData("uploads/image.jpg"),
        //    InlineData("/uploads/image.jpg"),
        //    InlineData("~/uploads/image.jpg")
        //]
        //public void ToPublicUrl_WithConfigNoProtocol(string imageUrl)
        //{
        //    var res = ExternalWebLinkResolver.ToPublicUrl(imageUrl, "//myserver.com/root");
        //    Assert.Equal("//myserver.com/root/uploads/image.jpg", res);
        //}
    }
}
