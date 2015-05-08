using N2.Details;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    /// <summary>
    /// Caching page is used as the base class for several pages that allow proxy caching
    /// </summary>
    public class CachingPageBase : PageModelBase
    {
        [EditableNumber(Title = "Max Age Cache Control", ContainerName = Defaults.Containers.Advanced, SortOrder = 800,
            HelpTitle = "Max Age Cache Control Setting", HelpText = "The value is input in seconds and will set the maximum age of the page’s caching life on the CDN.",
            DefaultValue = 300, MinimumValue = "0", MaximumValue = "86400",
            InvalidRangeText = "The maximum value is 86400, minimum 0",
            Required = true, RequiredText = "A value is required")]
        public virtual int CacheMaxAge { get; set; }
    }
}
