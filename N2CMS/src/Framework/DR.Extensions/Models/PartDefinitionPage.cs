using System;
using System.Collections.Generic;
using N2.Details;
using N2.Integrity;

namespace N2.Models
{
    [PageDefinition(title: "Part Definition", IconUrl = "{IconsUrl}/brick.png")]
    [RestrictParents(typeof(FolderPage))]
    [Web.Template("info", "{ManagementUrl}/Edit.aspx")]
    [WithEditableIdentifier]
    [WithEditableTitle]
    public class PartDefinitionPage : MetaDataPage
    {
        public override string IconUrl
        {
            get
            {
                return string.IsNullOrEmpty(Icon) ? base.IconUrl : Icon;
            }
        }

        // TODO allow parent class selection
        private const string DefaultHtmlFormat = @"@model {0}
        <div>
	        <span>your Razor template here</span>
        </div>";

        [EditableText(SortOrder = 100)]
        public virtual string Description { get; set; }

        [EditableText(DefaultValue = "{IconsUrl}/tag.png", SortOrder = 101)] // TODO add Icon selector
        public virtual string Icon { get; set; }

        // TODO validation, add model automatically
        [EditableRazor(Title = "HTML Template", MimeType = EditableSourceAttribute.SourceMimeType.Html, SortOrder = 102, ValidationText = "Bad Model")]
        public virtual string Template { get; set; }

        //[EditableSource(Title = "Definition JS", MimeType = EditableSourceAttribute.SourceMimeType.JavaScript, SortOrder = 103)]
        //public virtual string Definition { get; set; }

        [EditableNumber(SortOrder = 110, DefaultValue = 300)]
        public virtual int PartSortOrder { get; set; }

        [EditableChildren(Title = "Attributes", ZoneName = "Attributes", SortOrder = 210)]
        public virtual IList<AttributePart> Attributes
        {
            get { return GetChildren<AttributePart>("Attributes"); }
        }

        [PartDefinition("Attribute", IconUrl = "{IconsUrl}/tag.png")]
        [RestrictParents(typeof(PartDefinitionPage))]
        [AllowedZones("Attributes")]
        [WithEditableIdentifier(Prefix = "@Content.Data.")]
        [WithEditableTitle]
        public class AttributePart : PartModelBase
        {
            [EditableEnum(Title = "Type", EnumType = typeof(AttributePartTypeEnum), DefaultValue = AttributePartTypeEnum.Text, SortOrder = 100)]
            public AttributePartTypeEnum PartType
            {
                get { return GetDetail<AttributePartTypeEnum>("PartType", AttributePartTypeEnum.Text); }
                set { SetDetail("PartType", value, AttributePartTypeEnum.Text); }
            }

            [EditableText(SortOrder = 101)]
            public virtual string DefaultValue { get; set; }

            [EditableText(SortOrder = 102)]
            public virtual string HelpText { get; set; }

            // TODO: make extensible
            public enum AttributePartTypeEnum
            {
                Text,
                RichText,
                Number,
                CheckBox,
                Url,
                Image,
                List,
                Category,
                Product
                // TODO allow child class collections
            }
        }
    }
}
