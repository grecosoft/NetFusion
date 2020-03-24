using NetFusion.Messaging.Types;

namespace Demo.Subscriber.Events
{
    public class TemperatureReading : DomainEvent
    {
        public string Zip { get; set; }
        public decimal Reading { get; set; }
        public Coordinates Coordinates { get; set; }
    }

    public class Coordinates
    {
        public decimal North { get; set; }
        public decimal West { get; set; }
    }
}
