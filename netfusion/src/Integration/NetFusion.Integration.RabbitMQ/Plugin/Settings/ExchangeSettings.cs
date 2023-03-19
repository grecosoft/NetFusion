using NetFusion.Common.Base.Validation;

namespace NetFusion.Integration.RabbitMQ.Plugin.Settings
{
    /// <summary>
    /// Exchange settings specified within the application configuration.
    /// If specified they override the corresponding settings set in code.
    /// </summary>
    public class ExchangeSettings : IValidatableType
    {
        public string ExchangeName { get; set; } = null!;
        
        public bool? IsDurable { get; set; }
        public bool? IsAutoDelete { get; set; }
        public string? AlternateExchangeName { get; set; }
        
        /// <summary>
        /// Number of milliseconds after which a RPC request will timeout
        /// if a response is not received from the message consumer. The
        /// default value is 10 seconds.
        /// </summary>
        public int? CancelRpcRequestAfterMs { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(CancelRpcRequestAfterMs is null or > 0, 
                $"{nameof(CancelRpcRequestAfterMs)} must be greater then zero.");
        }
    }
}