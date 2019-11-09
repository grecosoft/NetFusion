using System;
using NetFusion.Messaging.Types;

namespace Demo.Subscriber.Events
{
    public class OrderSubmitted : DomainEvent
    {
        public string PartNumber { get; set; }
        public string State { get; set; }
        public int Quantity { get; set; }
        public DateTime? NeededBy { get; set; }
    }
}
