using System.ComponentModel.DataAnnotations;
using NetFusion.Base.Validation;

namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Exchange settings specified within the application configuration.
    /// If specified they override all corresponding settings set in code.
    /// </summary>
    public class ExchangeSettings : IValidatableType
    {
        /// <summary>
        /// The name of the exchange as specified in code.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Do not create an exchange. If the named exchange doesn't exist, throw an exception.
        /// </summary>
        public bool Passive { get; set; }

        /// <summary>
        /// Route messages to this exchange if they cannot be routed.
        /// </summary>
        public string AlternateExchange { get; set; }

        /// <summary>
        /// Number of milliseconds after which a RPC request will timeout
        /// if a response is not received from the message consumer. The
        /// default value is 10 seconds.
        /// </summary>
        public int CancelRpcRequestAfterMs { get; set; } = SettingDefaults.RpcTimeOutAfterMs;

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(! string.IsNullOrWhiteSpace(ExchangeName), $"{nameof(ExchangeName)} not specified.");
            
            validator.Verify(0 < CancelRpcRequestAfterMs, 
                $"{nameof(CancelRpcRequestAfterMs)} must be greater then zero.");
        }
    }
}