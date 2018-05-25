namespace WebApiHost.Models
{
    public class CustomerSummary : ContactSummary
    {
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
