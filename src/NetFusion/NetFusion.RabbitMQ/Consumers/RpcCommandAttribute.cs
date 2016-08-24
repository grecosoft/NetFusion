using NetFusion.Common;
using NetFusion.RabbitMQ.Core;
using System;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Used to specify that a command should be published to a consumer's queue
    /// that processes incoming RPC style messages.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RpcCommandAttribute : Attribute
    {
        /// <summary>
        /// The broker name to which the RPC command should be published.
        /// </summary>
        public string BrokerName { get; }

        /// <summary>
        /// The key that identifies the consumer's queue that processes RPC message commands.
        /// </summary>
        public string RequestQueueKey { get; }

        /// <summary>
        /// Used to specify a RPC style message command's properties.
        /// </summary>
        /// <param name="brokerName">The broker name to which the RPC command should be published.</param>
        /// <param name="requestQueueKey">The key that identifies the consumer's queue that processes RPC message commands.</param>
        public RpcCommandAttribute(string brokerName, string requestQueueKey)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));
            Check.NotNullOrWhiteSpace(requestQueueKey, nameof(requestQueueKey));

            this.BrokerName = brokerName;
            this.RequestQueueKey = requestQueueKey;
        }

        /// <summary>
        /// String property used to identity the type of the message to the consumer.
        /// If not specified, the name of the .NET type is used (this should be avoided
        /// if the consumer is not a .NET based client or a dynamic client like JavaScript).
        /// </summary>
        public string ExternalTypeName { get; set; }

        /// <summary>
        /// The content type used to serialize the message.  If not specified, the content
        /// type specified within the RpcConsumerSettings settings of the BrokerSettings 
        /// configuration is used.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Returns the property on the attribute than can be passed around.
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
