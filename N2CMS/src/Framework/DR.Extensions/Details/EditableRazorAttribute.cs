using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace N2.Details
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableRazorAttribute : EditableSourceAttribute
    {
        protected virtual Control AddCustomValidator(Control container, Control editor)
        {
            var cv = new CustomValidator();
            cv.ID = Name + "_rev";
            cv.ControlToValidate = editor.ID;
            cv.Display = ValidatorDisplay.Dynamic;
            cv.Text = GetLocalizedText("ValidationText") ?? ValidationText;
            cv.ErrorMessage = GetLocalizedText("ValidationMessage") ?? ValidationMessage;
            cv.ServerValidate += OnServerValidate;
            
            container.Controls.Add(cv);

            return cv;
        }

        //public delegate bool ServerValidationEventHandler(string value);
        //public event ServerValidationEventHandler OnValidation;
        private const string ExpectedModel = "N2.Models.ContentPart";

        private void OnServerValidate(object source, ServerValidateEventArgs args)
        {
            //args.IsValid = OnValidation(args.Value);
            var code = args.Value;
            var lines = code.Split(Environment.NewLine.ToCharArray());
            string model = null;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (s.StartsWith("@model"))
                {
                    model = s.Substring(6).Trim();
                    break;
                }                    
            }

            args.IsValid = (model == null) || (model.Equals(ExpectedModel));

            var cv = source as CustomValidator;
            if (cv != null && !args.IsValid)
                cv.ErrorMessage = "@model should be " + ExpectedModel;
        }

        protected override void AddValidation(Control container, Control editor)
        {
            //if (OnValidation != null)
                AddCustomValidator(container, editor);

            base.AddValidation(container, editor);
        }
    }
}