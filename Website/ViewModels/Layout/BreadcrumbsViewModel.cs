using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    [DataContract]
    [Serializable]
    public class Breadcrumb
    {
        [DataMember]
        public string Title {get; set;}
        [DataMember]
        public string Url { get; set; }
    }

    [DataContract]
    [Serializable]
    public class BreadcrumbCollection
    {
        [DataMember]
        public Breadcrumb[] Breadcrumbs { get; set; }
    }

    public class BreadcrumbViewModel : PageViewModelBase
    {
        public BreadcrumbViewModel()
        {
            Separator = "<span>></span>";
            Class = "activePage";
        }

        public string Class { get; set; }

        private readonly List<Breadcrumb> _crumbs = new List<Breadcrumb>();
        public List<Breadcrumb> Crumbs { get { return _crumbs; } }
        public string Separator { get; set; }
    }
}