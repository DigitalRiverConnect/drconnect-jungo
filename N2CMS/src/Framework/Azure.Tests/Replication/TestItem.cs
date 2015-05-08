using System;
using System.Collections.Generic;
using N2.Collections;
using N2.Definitions;
using N2.Details;
using N2.Integrity;
using N2.Persistence;
using N2.Persistence.Search;
using N2.Web;

namespace N2.Azure.Tests.Replication
{
    // used to track that flush gets invoked
    public class FakeFlushable : IFlushable
    {
        public void Flush()
        {
            Flushed = true;
        }

        public bool Flushed { get; set; }
    }

    [PageDefinition("Test persistable Item", Name = "TestItem")]
    public class TestItem : ContentItem, IPage
    {
        public virtual bool BoolProperty
        {
            get { return (bool)(GetDetail("BoolProperty") ?? true); }
            set { SetDetail<bool>("BoolProperty", value); }
        }

        [EditableNumber(DefaultValue = 666)]
        public virtual int IntProperty
        {
            get { return (int)(GetDetail("IntProperty") ?? 0); }
            set { SetDetail<int>("IntProperty", value); }
        }

        [EditableDate]
        public virtual DateTime DateTimeProperty
        {
            get { return (DateTime)(GetDetail("DateTimeProperty") ?? DateTime.MinValue); }
            set { SetDetail<DateTime>("DateTimeProperty", value); }
        }
        public virtual double DoubleProperty
        {
            get { return Convert.ToDouble(GetDetail("DoubleProperty") ?? 0); }
            set { SetDetail<double>("DoubleProperty", value); }
        }
        [Indexable]
        public virtual string StringProperty
        {
            get { return (string)(GetDetail("StringProperty") ?? string.Empty); }
            set { SetDetail<string>("StringProperty", value); }
        }

        public virtual ContentItem LinkProperty
        {
            get { return (ContentItem)GetDetail("LinkProperty"); }
            set { SetDetail<ContentItem>("LinkProperty", value); }
        }

        [Editable(Name = "ContentLinks", PersistAs = PropertyPersistenceLocation.DetailCollection)]
        public virtual IEnumerable<ContentItem> ContentLinks
        {
            get
            {
                var dc = GetDetailCollection("ContentLinks", false);
                if (dc == null)
                    return new ContentItem[0];

                return dc.Enumerate<ContentItem>();
            }
            set
            {
                var dc = GetDetailCollection("ContentLinks", true);
                dc.Replace(value);
            }
        }

        public virtual object ObjectProperty
        {
            get { return (object)GetDetail("ObjectProperty"); }
            set { SetDetail<object>("ObjectProperty", value); }
        }

        [EditableEnum]
        public virtual AppDomainManagerInitializationOptions EnumProperty
        {
            get { return GetDetail("EnumProperty", AppDomainManagerInitializationOptions.None); }
            set { SetDetail("EnumProperty", value, AppDomainManagerInitializationOptions.None); }
        }

        public virtual Guid GuidProperty
        {
            get
            {
                string value = GetDetail<string>("GuidProperty", null);
                return string.IsNullOrEmpty(value) ? Guid.Empty : new Guid(value);
            }
            set
            {
                SetDetail("GuidProperty", value.ToString());
            }
        }

        public virtual string WritableGuid
        {
            get { return (string)(GetDetail("WritableRSSString") ?? Guid.NewGuid().ToString()); }
            set { SetDetail("WritableRSSString", value, Guid.NewGuid().ToString()); }
        }

        public virtual string ReadOnlyGuid
        {
            get
            {
                string result = (string)GetDetail("ReadOnlyRSSString");
                if (string.IsNullOrEmpty(result))
                {
                    result = Guid.NewGuid().ToString();
                    SetDetail("ReadOnlyRSSString", result);
                }
                return result;
            }
        }

        [Indexable]
        public virtual string NonDetailProperty { get; set; }

        [Indexable]
        public virtual string NonDetailOnlyGetterProperty { get { return "Lorem ipsum"; } }

        [EditableTags]
        public virtual IEnumerable<string> Tags
        {
            get { return GetDetailCollection("Tags", true).OfType<string>(); }
            set { GetDetailCollection("Tags", true).Replace(value); }
        }

        [Persistable]
        public virtual string PersistableProperty { get; set; }

        //[EditableDummy]
        //public virtual ContentItemTests.CloneableList<string> StringList { get; set; }

        [EditableLink]
        public virtual ContentItem EditableLink { get; set; }

        // override to avoid use of N2.Context.Current.UrlParser.BuildUrl(this);
        public override string Url
        {
            get { return ""; }
        }        
    }

    [PartDefinition("Test Link")]
    [RestrictParents(typeof(ListPart))]
    [AllowedZones("Links")] // ensure not added on pages directly
    public class ListItem : ContentItem
    {
        public string LinkText
        {
            get { return ((string)GetDetail("LinkText") ?? ""); }
            set { SetDetail("LinkText", value, ""); }
        }

        public override string Url
        {
            get { return ""; }
        }
    }

    [PartDefinition("List of Links")]
    public class ListPart : ContentItem
    {
        public virtual IList<T> GetChildren<T>(string zoneName) where T : ContentItem
        {
            return new ItemList<T>(Children,
                                          new AccessFilter(),
                                          new TypeFilter(typeof(T)),
                                          new ZoneFilter(zoneName));
        }

        public IList<ListItem> Links
        {
            get { return GetChildren<ListItem>("Links"); }
        }

        public override string Url
        {
            get { return ""; }
        }
    }
}