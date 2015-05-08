using System;

namespace N2.Models
{
    public class SiteMapEntry
    {
        public SiteMapEntry()
        {
            ChangeFrequency = ChangeFrequencyEnum.Undefined;
            Priority = 0.5;
        }
        /// <summary>
        /// Provides the full URL of the page, including the protocol (e.g. http, https) and a trailing slash, 
        /// if required by the site's hosting server. This value must be less than 2,048 characters.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// How frequently the page may change
        /// </summary>
        public ChangeFrequencyEnum ChangeFrequency { get; set; }

        /// <summary>
        /// The valid range is from 0.0 to 1.0, with 1.0 being the most important. The default value is 0.5.
        /// </summary>
        public double Priority { get; set; }

        /// <summary>
        /// The date that the file was last modified
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Title Non-Standard
        /// </summary>
        public string Title { get; set; }

    }

    [Serializable]
    public enum ChangeFrequencyEnum
    {
        Undefined = 0,
        Always = 1, // "Always" is used to denote documents that change each time that they are accessed. 
        Never = 2, // "Never" is used to denote archived URLs (i.e. files that will not be changed again).
        Hourly = 3,
        Daily = 4,
        Weekly = 5,
        Monthly = 6,
        Yearly = 7
    }

}