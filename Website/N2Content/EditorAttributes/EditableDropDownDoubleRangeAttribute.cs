using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using N2.Details;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.EditorAttributes
{
    public class EditableDropDownDoubleRangeAttribute : EditableDropDownAttribute
    {
        public EditableDropDownDoubleRangeAttribute()
        {
            MinimumValue = 0.0;
            MaximumValue = 1.0;
            Increment = 0.1;
            NumberFormat = "F01";
        }

        public double MinimumValue { get; set; }
        public double MaximumValue { get; set; }
        public double Increment { get; set; }
        public string NumberFormat { get; set; }

        protected override ListItem[] GetListItems()
        {
            var items = new List<ListItem>();
            for (var i = MinimumValue; i <= MaximumValue; i += Increment)
            {
                var istr = i.ToString(NumberFormat, CultureInfo.CurrentUICulture);
                items.Add(new ListItem {Text = istr, Value = istr});
            }
            return items.ToArray();
        }
    }
}
