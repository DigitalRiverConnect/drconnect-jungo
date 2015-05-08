using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2.Resources;

namespace N2.Details
{
	[AttributeUsage(AttributeTargets.Property)]
	public class EditableSourceAttribute : EditableTextBoxAttribute
	{
        public enum SourceMimeType
        {
            None,
            Html,
            Css,
            JavaScript,
            XML
        }

        private string GetMimeType()
        {
            switch (MimeType)
            {
                case SourceMimeType.None:
                    return "text/plain";
                case SourceMimeType.Html:
                    return "text/html";
                case SourceMimeType.Css:
                    return "text/css";
                case SourceMimeType.JavaScript:
                    return "text/javascript";
                case SourceMimeType.XML:
                    return "text/xml";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

		public EditableSourceAttribute()
			: base(null, 100)
		{
            MimeType = SourceMimeType.Html;
		}

		public EditableSourceAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
            MimeType = SourceMimeType.Html;
		}

        public SourceMimeType MimeType { get; set; }

		protected override TextBox CreateEditor()
		{
            TextMode = TextBoxMode.MultiLine; 

			var ed = new TextBox {TextMode = TextMode}; // TextBox or FreeTextArea?
		    return ed;
		}

        protected override Control AddEditor(Control container)
        {
            var editor = base.AddEditor(container);

            // enable codemirror
            string mime = GetMimeType();
            const string baseUrl = "{ManagementUrl}/Resources/ckeditor/plugins/codemirror";

            var script = "var cmelem = document.getElementById(\"" + editor.ClientID + "\"); "
                       + "if (cmelem) { var cmeditor = CodeMirror.fromTextArea(cmelem, "
                       + "{lineNumbers: true, matchBrackets: true, mode: \"" + mime + "\"}); }";

            container.Page.StyleSheet(baseUrl + "/css/codemirror.min.css");
            container.Page.JavaScript(baseUrl + "/js/codemirror.min.js");

            if (MimeType == SourceMimeType.XML || MimeType == SourceMimeType.Html)
                container.Page.JavaScript(baseUrl + "/js/mode/xml.js");
            if (MimeType == SourceMimeType.JavaScript || MimeType == SourceMimeType.Html)
                container.Page.JavaScript(baseUrl + "/js/mode/javascript.js");
            if (MimeType == SourceMimeType.Css || MimeType == SourceMimeType.Html)
                container.Page.JavaScript(baseUrl + "/js/mode/css.js");
            if (MimeType == SourceMimeType.Html)
                container.Page.JavaScript(baseUrl + "/js/mode/htmlmixed.js");
            container.Page.JavaScript(script, ScriptOptions.DocumentReady);

            return editor;
        }

		protected override Control AddRequiredFieldValidator(Control container, Control editor)
		{
			var rfv = (RequiredFieldValidator)base.AddRequiredFieldValidator(container, editor);
		    rfv.EnableClientScript = false;
		    return rfv;
		}
	}
}
