using System;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Models
{
    public class DiagnosticLock
    {
        public bool IsLocked { get; set; }
        public TimeSpan LockDuration { get; set; }
    }
}
