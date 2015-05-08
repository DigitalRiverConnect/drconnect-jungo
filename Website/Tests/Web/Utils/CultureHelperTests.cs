using System.Linq;
using DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure.Helpers;
using Shouldly;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Web.Utils
{
	public class CultureHelperTests : TestBase
	{
        public CultureHelperTests()
		{
			//DependencyRegistrar.StandardDependencies();
		}

	    private static string[] userlangs = {"en-us; q=0.8", "de-de"};

	    [Fact]
        public void Test_GetLocalesFromUserLanguages()
        {
            var result1 = CultureHelper.GetLocalesFromUserLanguages(userlangs);
            result1.Count().ShouldBe(2);
            result1.First().ShouldBe("en-us");

            var result2 = CultureHelper.GetLocalesFromUserLanguages(null);
            result2.Count().ShouldBe(0);
        }
	}
}
