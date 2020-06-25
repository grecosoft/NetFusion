
using System;
using NetFusion.Rest.Resources;

namespace Service.WebApi.Models
{
    /// <summary>
    /// Model representing a payment.
    /// </summary>
    [ExposedName("type-payment")]
    public class PaymentModel
    {
        /// <summary>
        /// The amount of the received payment.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The date at which the payment was posted to the ledger.
        /// </summary>
        public DateTime PostedDate { get; set; }
    }
}
