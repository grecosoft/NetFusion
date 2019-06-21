using System;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Events
{
    public class ClaimStatusUpdated : DomainEvent
    {
        public string InsuredId { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime NextStatusUpdate { get; set; }
        public string NextStatus { get; set; }
    }
}
