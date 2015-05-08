using System.Runtime.Serialization;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Catalog
{
    public class VariationsCollection
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "levels")]
        public string[] Levels { get; set; }

        [DataMember(Name = "variations")]
        public Variation Variations { get; set; }

        [DataContract]
        public class Variation
        {
            public Variation()
            {
                Options = new Option[0];
            }

            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "options")]
            public Option[] Options { get; set; }
        }

        [DataContract]
        public class Option
        {
            [DataMember(Name = "text")]
            public string Text { get; set; }

            [DataMember(Name = "value")]
            public string Value { get; set; }

            [DataMember(Name = "product")]
            public Product VariationProduct { get; set; }

            [DataMember(Name = "variations")]
            public Variation Variations { get; set; }
        }

    }
}

    