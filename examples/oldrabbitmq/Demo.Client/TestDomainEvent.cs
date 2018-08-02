using NetFusion.Messaging.Types;

namespace Demo.Client
{
    public class TestDomainEvent : DomainEvent
    {
        public string Make { get; set; }
        public string Model { get; set; }
    }
}