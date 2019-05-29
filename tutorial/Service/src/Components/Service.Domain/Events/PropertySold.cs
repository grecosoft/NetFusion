namespace Service.Domain.Events
{
    using NetFusion.Messaging.Types;

    public class PropertySold : DomainEvent
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public decimal AskingPrice { get; set; }
        public decimal SoldPrice { get; set; }
    }
}