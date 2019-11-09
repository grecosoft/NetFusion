namespace Demo.Domain.Entities
{
    public class CustomerInfo
    {
        public Customer Customer { get; set; }
        public AutoSuggestion[] Suggestions { get; set; }
    }
}