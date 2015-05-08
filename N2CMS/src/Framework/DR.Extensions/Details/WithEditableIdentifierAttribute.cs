using System;
using System.Web.UI;
using N2.Web.UI.WebControls;

namespace N2.Details
{
    /// <summary>Class applicable attribute used to add a name editor. The name represents the URL slug for a certain content item.</summary>
    /// <example>
    /// [N2.Details.WithEditableName("Address name", 20)]
    /// public abstract class AbstractBaseItem : N2.ContentItem 
    /// {
    ///	}
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WithEditableIdentifierAttribute : AbstractEditableAttribute //, IWritingDisplayable
    {
        /// <summary>
        /// Creates a new instance of the WithEditableAttribute class with default values.
        /// </summary>
        public WithEditableIdentifierAttribute()
            : this("Identifier", -10)
        {
        }
        /// <summary>
        /// Creates a new instance of the WithEditableAttribute class with default values.
        /// </summary>
        /// <param name="title">The label displayed to editors</param>
        /// <param name="sortOrder">The order of this editor</param>
        public WithEditableIdentifierAttribute(string title, int sortOrder)
            : base(title, "Name", sortOrder)
        {
            KeepUpdatedToolTip = "Keep the identifier in sync with Title";
            KeepUpdatedText = "";
        }

        /// <summary>Allow editor to choose wether to update name automatically.</summary>
        public bool? ShowKeepUpdated { get; set; }

        /// <summary>The text on the keep updated check box.</summary>
        public string KeepUpdatedText { get; set; }

        /// <summary>The tool tip on the keep updated check box.</summary>
        public string KeepUpdatedToolTip { get; set; }

        public string Prefix { get; set; }

        /// <summary>Sets focus on the name editor.</summary>
        public bool Focus { get; set; }

        public override bool UpdateItem(ContentItem item, Control editor)
        {
            var ne = (NameEditor)editor;
            if (item.Name != ne.Text)
            {
                item.Name = ne.Text;
                return true;
            }
            return false;
        }

        public override void UpdateEditor(ContentItem item, Control editor)
        {
            var ne = (NameEditor)editor;
            ne.Text = item.Name;
            ne.Prefix = Prefix;
            ne.Suffix = "";
        }

        protected override Control AddEditor(Control container)
        {
            var ne = new NameEditor
                {
                    ID = Name,
                    CssClass = "nameEditor",
                    WhitespaceReplacement = ' ', // gets trimmed to '' 
                    ToLower = false,
                    ShowKeepUpdated = ShowKeepUpdated
                };
            ne.KeepUpdated.Text = KeepUpdatedText;
            ne.KeepUpdated.ToolTip = KeepUpdatedToolTip;
            ne.Placeholder(GetLocalizedText("FromDatePlaceholder") ?? Placeholder);
            container.Controls.Add(ne);
            if (Focus) ne.Focus();
            return ne;
        }

        //#region IWritingDisplayable Members

        //public void Write(ContentItem item, string propertyName, System.IO.TextWriter writer)
        //{
        //    writer.Write(item[propertyName]);
        //}

        //#endregion
    }
}
