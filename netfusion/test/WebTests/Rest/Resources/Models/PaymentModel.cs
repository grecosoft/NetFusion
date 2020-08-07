using NetFusion.Rest.Resources;

namespace WebTests.Rest.Resources.Models
{
    /// <summary>
    /// Represents a model returned from a REST Api.
    /// </summary>
    [Resource("PaymentResource")]
    public class PaymentModel
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
    }
}