using NetFusion.Rest.Resources;

namespace WebTests.Rest.Resources.Models
{
    [Resource("AccountResource")]
    public class AccountModel
    {
        public string AccountNumber { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}