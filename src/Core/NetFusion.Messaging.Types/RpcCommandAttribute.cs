using System;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Used to specify that a command should be published to a consumer's queue that processes 
    /// incoming RPC style messages.  This attribute is applied to a class deriving from ICommand.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RpcCommandAttribute : Attribute
    {
        /// <summary>
        /// The broker name to which the RPC command should be published.
        /// Corresponds to value specified in configuration.
        /// </summary>
        public string BrokerName { get; }

        /// <summary>
        /// The key that identifies the consumer's queue that processes RPC message commands.
        /// Corresponds to value specified in configuration.
        /// </summary>
        public string RequestQueueKey { get; }

        /// <summary>
        /// String property used to identity the type of the message to the consumer.
        /// </summary>
        public string ExternalTypeName { get; }

        /// <summary>
        /// Used to specify a RPC style message command's properties.
        /// </summary>
        /// <param name="brokerName">The configuration specified broker name to which the RPC command should be published.</param>
        /// <param name="requestQueueKey">The key that identifies the consumer's queue that processes RPC message commands.</param>
        /// <param name="externalTypeName">The name used to represent the message to external consumers.</param>
        public RpcCommandAttribute(string brokerName, string requestQueueKey, string externalTypeName)
        {
            BrokerName = brokerName ?? throw new ArgumentNullException(nameof(brokerName));
            RequestQueueKey = requestQueueKey ?? throw new ArgumentNullException(nameof(requestQueueKey)); 
            ExternalTypeName = externalTypeName ?? throw new ArgumentNullException(nameof(externalTypeName));
        }

        /// <summary>
        /// Used to specify a RPC style message command's properties. This version of the constructor is used on
        /// the receiving end of the RPC call by the consumer.
        /// </summary>
        /// <param name="externalTypeName">The name used to represent the message to external consumers.</param>
        public RpcCommandAttribute(string externalTypeName)
        {            
            ExternalTypeName = externalTypeName ?? throw new ArgumentNullException(nameof(externalTypeName));
        }

        /// <summary>
        /// The content type used to serialize the message.  If not specified, the content
        /// type specified within the RpcConsumerSettings settings of the BrokerSettings 
        /// configuration is used (if not specified directly on the message).
        /// </summary>
        public string ContentType { get; set; }
    }
}
