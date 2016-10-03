using NetFusion.Common;
using NetFusion.RabbitMQ.Core.Rpc;
using System;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Used to specify that a command should be published to a consumer's queue
    /// that processes incoming RPC style messages.  This attribute is applied
    /// to a class deriving from ICommand.
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
        /// Used to specify a RPC style message command's properties.
        /// </summary>
        /// <param name="brokerName">The configuration specified broker name to which the RPC command should be published.</param>
        /// <param name="requestQueueKey">The key that identifies the consumer's queue that processes RPC message commands.</param>
        /// <param name="externalTypeName">The name used to represent the message to external consumers.</param>
        public RpcCommandAttribute(string brokerName, string requestQueueKey, string externalTypeName)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));
            Check.NotNullOrWhiteSpace(requestQueueKey, nameof(requestQueueKey));
            Check.NotNullOrWhiteSpace(externalTypeName, nameof(externalTypeName));

            this.BrokerName = brokerName;
            this.RequestQueueKey = requestQueueKey;
            this.ExternalTypeName = externalTypeName;
        }

        /// <summary>
        /// String property used to identity the type of the message to the consumer.
        /// </summary>
        public string ExternalTypeName { get; set; }

        /// <summary>
        /// The content type used to serialize the message.  If not specified, the content
        /// type specified within the RpcConsumerSettings settings of the BrokerSettings 
        /// configuration is used (if not specified directly on the message).
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Returns the property on the attributes so calling code is not coded to
        /// and attribute.
        /// </summary>
        /// <returns></returns>
        public RpcProperties ToRpcProps()
        {
            return new RpcProperties
            {
                ContentType = this.ContentType,
                ExternalTypeName = this.ExternalTypeName
            };
        }
    }
}
