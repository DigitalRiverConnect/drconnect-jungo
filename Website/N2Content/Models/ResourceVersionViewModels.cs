using System;
using Newtonsoft.Json;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    public class ResourceVersionSiteInfo
    {
        [JsonProperty("c")]
        public string Company { get; set; }
        [JsonProperty("s")]
        public string Site { get; set; }
        [JsonProperty("cc")]
        public string CultureCode { get; set; }
    }

    public class ResourceVersionSearchInfo
    {
        [JsonProperty("tc")]
        public string ResourceTypeContains { get; set; }
        [JsonProperty("kc")]
        public string ResourceKeyContains { get; set; }
        [JsonProperty("vc")]
        public string ResourceValueContains { get; set; }
    }

    public class ResourceVersionPagingInfo
    {
        [JsonProperty("ti")]
        public int TotalItems { get; set; }
        [JsonProperty("ipp")]
        public int ItemsPerPage { get; set; }
        [JsonProperty("cp")]
        public int CurrentPage { get; set; }
        [JsonIgnore]
        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
    }

    public class ResourceVersionEditorContext
    {
        public ResourceVersionEditorContext()
        {
            SiteInfo = new ResourceVersionSiteInfo();
            SearchInfo = new ResourceVersionSearchInfo();
            PagingInfo = new ResourceVersionPagingInfo();
        }
        [JsonProperty("sii")]
        public ResourceVersionSiteInfo SiteInfo { get; set; }
        [JsonProperty("sei")]
        public ResourceVersionSearchInfo SearchInfo { get; set; }
        [JsonProperty("pi")]
        public ResourceVersionPagingInfo PagingInfo { get; set; }
        [JsonProperty("t")]
        public string ResourceType { get; set; }
        [JsonProperty("k")]
        public string ResourceKey { get; set; }
        [JsonProperty("u")]
        public string N2User { get; set; }
        [JsonProperty("pc")]
        public int PreviewCount { get; set; }
    }

    public class ResourceVersionInfo
    {
        public int VersionId { get; set; }
        public int Version { get; set; }
        public bool IsPreview { get; set; }
        public string ResourceValue { get; set; }
        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class ResourceVersionViewModel
    {
        public string Company { get; set; }
        public string Site { get; set; }
        public string CultureCode { get; set; }
        public string ResourceType { get; set; }
        public string ResourceKey { get; set; }
        public string ResourceValue { get; set; }
        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
        public ResourceVersionInfo[] Versions { get; set; } 
    }

    public class ResourceVersionInfoWithTypeKey : ResourceVersionInfo
    {
        public string ResourceType { get; set; }
        public string ResourceKey { get; set; }
    }

    public class ResourceVersionEditViewModel : ResourceVersionInfoWithTypeKey
    {
        public string SystemValue { get; set; }
    }
}
