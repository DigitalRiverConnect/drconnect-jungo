using System.Collections.Generic;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models
{
    public class DiagnosticTreeNode
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsPage { get; set; }
        public List<DiagnosticTreeNode> Children { get; set; }
    }
}
