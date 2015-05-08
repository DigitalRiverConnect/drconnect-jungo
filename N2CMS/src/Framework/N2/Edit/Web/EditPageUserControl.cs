using System.Web.UI;
using N2.Engine;

namespace N2.Edit.Web
{
    /// <summary>
    /// A user control that provides access to the edit item page.
    /// </summary>
	public class EditPageUserControl : UserControl
	{
		public IEngine Engine
		{
			get { return (base.Page as EditPage) != null ? (base.Page as EditPage).Engine : N2.Context.Current; }
        }

        SelectionUtility _selection;
        protected SelectionUtility Selection
        {
            get
            {
                if (_selection == null)
                {
                    var editPage = base.Page as EditPage; // optimize selection resolution
                    _selection = editPage != null ? editPage.Selection : new SelectionUtility(this, Engine);
                }
                return _selection;
            }
            set { _selection = value; }
        }
	}
}
