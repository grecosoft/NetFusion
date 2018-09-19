namespace Demo.WebApi
{
    using NetFusion.Messaging.Types;

    public class AutoPurchasedEvent : DomainEvent
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
}
}