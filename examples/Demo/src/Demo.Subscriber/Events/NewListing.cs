using NetFusion.Messaging.Types;

namespace Demo.Subscriber.Events
{
    public class NewListing : DomainEvent
    {
        public string DataSource { get; set; }
        public string SourceId { get; set; }
        public decimal ListedPrice { get; set; }
        public string Realtor { get; set; }
        public int SquareFeet { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
    }
}