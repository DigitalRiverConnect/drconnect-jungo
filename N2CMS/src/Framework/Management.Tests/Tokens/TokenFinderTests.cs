using System;
using System.Linq;
using NUnit.Framework;
using N2.Web.Tokens;
using System.Web.Mvc;
using Shouldly;
using N2.Web.Mvc;

namespace N2.Tests.Web.Tokens
{
    [TestFixture]
    public class TokenFinderTests
    {
        private ViewEngineCollection viewEngines;
        private TokenDefinitionFinder finder;
        private Fakes.FakeProvider<ViewEngineCollection> provider;

        [SetUp]
        public void SetUp()
        {
            viewEngines = new ViewEngineCollection();
            provider = new Fakes.FakeProvider<ViewEngineCollection>(() => viewEngines);
            finder = new TokenDefinitionFinder(new Fakes.FakeWebContextWrapper(), provider);
        }

        [Test, Ignore("finds nothing")]
        public void Find_BuiltInTokens()
        {
            viewEngines.RegisterTokenViewEngine();
            
            var definitions = finder.FindTokens().ToList();

            definitions.Single().Name.ShouldBe("Hello");
        }
    }
}
