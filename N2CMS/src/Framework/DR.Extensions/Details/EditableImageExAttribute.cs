using System;
using System.IO;
using System.Web;
using N2.Interfaces;
using N2.Web.Drawing;

namespace N2.Details
{
    /// <summary>
    /// Allows to upload or select an image file to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableImageExAttribute : EditableImageUploadAttribute, IContentEditable
    {
        public EditableImageExAttribute()
            : this(null, 41)
        {
        }

        public EditableImageExAttribute(string title, int sortOrder)
            : base(title, sortOrder)
        {
        }

        #region IContentEditable Members

        public string GetEditorCssClass()
        {
            return "imageedit";
        }

        #endregion

        public override void Write(ContentItem item, string propertyName, TextWriter writer)
        {
            var filePath = item[propertyName] as string;
            if (string.IsNullOrEmpty(filePath))
                return;

            string url = Context.Current.Container.Resolve<IExternalWebLinkResolver>().GetPublicUrl(filePath);
            if (string.IsNullOrWhiteSpace(url))
                return;

            var extension = VirtualPathUtility.GetExtension(url);
            switch (ImagesUtility.GetExtensionGroup(extension))
            {
                case ImagesUtility.ExtensionGroups.Images:
                    writer.Write("<img src=\"{0}\" alt=\"{1}\"  />", url, Alt);
                    return;
                default:
                    writer.Write("<a href=\"{0}\">{1}</a>", url, VirtualPathUtility.GetFileName(url));
                    return;
            }
        }
    }
}
