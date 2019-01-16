namespace Service.Client.Events
{
    using NetFusion.Messaging.Types;

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