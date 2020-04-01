using NetFusion.Messaging.Types;

namespace Subscriber.WebApi.Events
{
    public class AutoSaleCompleted : DomainEvent
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public bool IsNew { get; set; }
    }
}