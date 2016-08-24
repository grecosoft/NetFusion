using NetFusion.RabbitMQ.Serialization;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Settings for a specific RPC consumer.
    /// </summary>
    public class RpcConsumerSettings 
    {
        /// <summary>
        /// A string constant to reference the queue in code.
        /// </summary>
        public string RequestQueueKey { get; set; }

        /// <summary>
        /// The actual queue name defined by the consumer to which RPC
        /// style messages will be published.
        /// </summary>
        public string RequestQueueName { get; set; }

        /// <summary>
        /// The default content type to which the messages should be serialized.
        /// </summary>
        public string ContentType { get; set; } = SerializerTypes.Json;

        /// <summary>
        /// The number of milliseconds after which a pending style RPC message
        /// should be canceled when no reply has been received.
        /// </summary>
        public int CancelRequestAfterMs { get; set; } = 10000;
    }
}
