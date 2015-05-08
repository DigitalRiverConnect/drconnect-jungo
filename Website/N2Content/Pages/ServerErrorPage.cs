using System;
using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [Obsolete]
    [PageDefinition("Custom Server Error Page",
        Description = "Custom server error display",
        SortOrder = 270,
        InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        IconUrl = "~/Content/img/icons/servererror16.png")]
    [WithEditableName]
    public class ServerErrorPage : PageModelBase
    {
        [EditableText(Title = "Heading", SortOrder = 100, HelpText = "A custom message to appear on the page, such as \"Sorry for the inconvenience...\".")]
        public virtual string Heading
        {
            get { return (string)(GetDetail("Heading") ?? ""); }
            set { SetDetail("Heading", value, ""); }
        }

        [EditableCheckBox(Title = "Show RequestID", SortOrder = 102, HelpText = "The RequestID is a unique number identifing the request.")]
        public virtual bool ShowRequestID
        {
            get { return (bool)(GetDetail("ShowRequestID") ?? false); }
            set { SetDetail("ShowRequestID", value, false); }
        }
   
        [EditableCheckBox(Title = "Show URL", SortOrder = 104, HelpText = "URL is the page where the error occurred.")]
        public virtual bool ShowUrl
        {
            get { return (bool)(GetDetail("ShowUrl") ?? false); }
            set { SetDetail("ShowUrl", value, false); }
        }

        [EditableCheckBox(Title = "Show Exception", SortOrder = 106, HelpText = "Exception is the low-level system error. This is available in the log but can be shown on this page if desired.")]
        public virtual bool ShowExcp
        {
            get { return (bool)(GetDetail("ShowExcp") ?? false); }
            set { SetDetail("ShowExcp", value, false); }
        }

        [EditableCheckBox(Title = "Show Stack Trace", SortOrder = 108, HelpText = "Stack Trace shows details of where the error occurred. This is available in the log but can be shown on this page if desired.")]
        public virtual bool ShowStackTrace
        {
            get { return (bool)(GetDetail("ShowStackTrace") ?? false); }
            set { SetDetail("ShowStackTrace", value, false); }
        }
    }
}