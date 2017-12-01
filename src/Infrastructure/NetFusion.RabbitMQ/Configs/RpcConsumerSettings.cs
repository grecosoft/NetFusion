using NetFusion.Base;
using NetFusion.Base.Validation;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Settings for a specific RPC consumer.  This is the information for the queues that 
    /// are exposed by the consumer to which the application containing this configuration
    /// can publish RPC messages to.  
    /// </summary>
    public class RpcConsumerSettings : IValidatableType
    {
        /// <summary>
        /// A string constant to reference the queue in code.  This value is specified by
        /// the publisher and should not be changed.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Request Queue Key is Required")]
        public string RequestQueueKey { get; set; }

        /// <summary>
        /// The actual queue name defined by the consumer to which RPC style messages will be published.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Request Queue Name is Required")]
        public string RequestQueueName { get; set; }

        /// <summary>
        /// The default content-type to which the messages should be serialized.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Content Type is Required")]
        public string ContentType { get; set; } = SerializerTypes.Json;

        /// <summary>
        /// The number of milliseconds after which a pending style RPC message
        /// should be canceled when no reply has been received.
        /// </summary>
        public int CancelRequestAfterMs { get; set; } = 10000;

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(this.CancelRequestAfterMs > 0,
                "Cancel Request After Milliseconds must be Greater than 0.",
                ValidationTypes.Error);
        }
    }
}
