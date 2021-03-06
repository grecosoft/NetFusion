using NetFusion.Base.Validation;

namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Exchange settings specified within the application configuration.
    /// If specified they override the corresponding settings set in code.
    /// </summary>
    public class ExchangeSettings : IValidatableType
    {
        /// <summary>
        /// The name of the exchange as specified in code.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Route messages to this exchange if they cannot be routed.
        /// </summary>
        public bool? IsNonRoutedSaved { get; set; }
        
        /// <summary>
        /// The MIME value indicating how the message should be serialized.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Number of milliseconds after which a RPC request will timeout
        /// if a response is not received from the message consumer. The
        /// default value is 10 seconds.
        /// </summary>
        public int? CancelRpcRequestAfterMs { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(! string.IsNullOrWhiteSpace(ExchangeName), $"{nameof(ExchangeName)} not specified.");

            validator.Verify(CancelRpcRequestAfterMs is null or > 0, 
                $"{nameof(CancelRpcRequestAfterMs)} must be greater then zero.");
        }
    }
}