using System;

namespace Jungo.Infrastructure.Config.Models
{
    [Serializable]
    public class ProductVariationsConfig
    {
        public VariationGroup[] Groups { get; set; }

        [Serializable]
        public class VariationGroup
        {
            public Variation[] Variations { get; set; }
        }

        [Serializable]
        public class Variation
        {
            public string Id { get; set; }
            public string AttributeName { get; set; }
            public Mask[] Masks { get; set; }
            public Alias[] Aliases { get; set; }
        }

        [Serializable]
        public class Mask
        {
            public string Value { get; set; }
            public string Format { get; set; }
            public string FormatVar0 { get; set; }
            public string FormatVar1 { get; set; }
        }

        [Serializable]
        public class Alias
        {
            public string Value { get; set; }
            public string AttributeName { get; set; }
        }
    }
}
