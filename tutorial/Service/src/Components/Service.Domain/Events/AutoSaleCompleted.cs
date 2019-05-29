namespace Service.Domain.Events
{
    using NetFusion.Messaging.Types;

    public class AutoSaleCompleted : DomainEvent
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public bool IsNew { get; set; }
    }
}