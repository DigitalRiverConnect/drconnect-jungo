using DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog;
using Xunit;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Tests.Models.ViewModels
{
    public class SortCategoryViewModelTests : TestBase
    {
        #region SelectedText

        // test: SelectedText returns null, doesn't blow up, when nothing in list
        [Fact]
        public void SelectedText_ListEmpty()
        {
            // setup
            var mod = new SortCategoryViewModel();

            // sense
            Assert.Equal(null, mod.SelectedText);
        }

        // test: SelectedText returns null, doesn't blow up, when list contains stuff but none selected
        [Fact]
        public void SelectedText_None()
        {
            // setup
            var mod = new SortCategoryViewModel();
            mod.Items.Add(new Option {Selected = false, Name = "t1", Value = "v1"});
            mod.Items.Add(new Option { Selected = false, Name = "t2", Value = "v1" });
            mod.Items.Add(new Option { Selected = false, Name = "t3", Value = "v1" });

            // sense
            Assert.Equal(null, mod.SelectedText);
        }

        // test: SelectedText returns selected item's text
        [Fact]
        public void SelectedText_Sel()
        {
            // setup
            var mod = new SortCategoryViewModel();
            mod.Items.Add(new Option { Selected = false, Name = "t1", Value = "v1" });
            mod.Items.Add(new Option { Selected = true, Name = "t2", Value = "v1" });
            mod.Items.Add(new Option { Selected = false, Name = "t3", Value = "v1" });

            // sense
            Assert.Equal("t2", mod.SelectedText);
        }

        #endregion
    }
}
