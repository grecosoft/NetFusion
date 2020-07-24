using NetFusion.Rest.Resources;

namespace WebTests.Rest.Resources.Models
{
    [Resource("PaymentResource")]
    public class PaymentModel
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
    }
}