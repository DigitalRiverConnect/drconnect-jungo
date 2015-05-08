using System.IO;
using DigitalRiver.CloudLink.Commerce.Nimbus.Mvc.Web.Utils;
using N2.DigitalRiver.Models.Helpers;
using Shouldly;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Web.Utils
{
	public class HtmlUtilsTests : TestBase
	{
        #region Data
        private const string TestHtml = @"<section class=""row-blue row-padded"">
    <div class=""grid-container"">
        <div class=""grid-row row-4"">
            <div class=""grid-unit col-1"">
                <div bi:type=""list"" class=""list-of-links list-of-links-lg list-array"">
                    <h2 bi:titleflag=""t1"" bi:title=""t1"" class=""heading"">Follow us</h2>
                    <ul bi:parenttitle=""t1"">
                        <li><a bi:index=""0"" bi:linkid=""SOC-00-000000"" bi:cpid=""hpSocial""
                               href=""http://www.facebook.com/microsoft"">Facebook</a>
                        </li>
                        <li><a bi:index=""1"" bi:linkid=""SOC-00-000000"" bi:cpid=""hpSocial""
                               href=""http://www.twitter.com/microsoft"">Twitter</a>
                        </li>
                        <li><a bi:index=""2"" bi:linkid=""SOC-00-000000"" bi:cpid=""hpSocial""
                               href=""http://www.microsoft.com/news"">News Center</a>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</section>";

	    private const string CleanHtml = @"<section class=""row-blue row-padded"">
    <div class=""grid-container"">
        <div class=""grid-row row-4"">
            <div class=""grid-unit col-1"">
                <div class=""list-of-links list-of-links-lg list-array"">
                    <h2 class=""heading"">Follow us</h2>
                    <ul>
                        <li><a href=""http://www.facebook.com/microsoft"">Facebook</a>
                        </li>
                        <li><a href=""http://www.twitter.com/microsoft"">Twitter</a>
                        </li>
                        <li><a href=""http://www.microsoft.com/news"">News Center</a>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</section>";
        #endregion 

		[Fact]
        public void RemoveNamespacedAttrsFromHtml_Works()
		{
		    var clean = HtmlHelper.RemoveNamespacedAttrsFromHtml(TestHtml);
		    clean.ShouldBe(CleanHtml);
		}

        // used to do manual cleanup
        //[Fact]
        //public void RemoveNamespacedAttrsFromHtmlFile()
        //{
        //    var raw = File.ReadAllText("d:/test.html");
        //    var clean = HtmlUtils.RemoveNamespacedAttrsFromHtml(raw);
        //    File.WriteAllText("d:/testout.html", clean);
        //}

	}
}
