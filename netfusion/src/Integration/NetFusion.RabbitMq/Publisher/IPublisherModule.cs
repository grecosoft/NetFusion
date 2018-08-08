using System;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Specifies the interface for the module responsible for determining the 
    /// exchanges to which a published message of a given type should be delivered.
    /// </summary>
    public interface IPublisherModule : IPluginModuleService
    {
        /// <summary>
        /// Determine if the specified message type has a defined exchange.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <returns>True if there is an exchange associated with the
        /// message type.  Otherwise, False is returned.</returns>
        bool IsExchangeMessage(Type messageType);

        /// <summary>
        /// Return the exchange definition metadata associated with a
        /// specific message type.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <returns>The exchange metadata or an exception if there are
        /// no exchanged asociated with the message type.</returns>
        ExchangeMeta GetDefinition(Type messageType);

        /// <summary>
        /// Return the configured RPC client to use when sending RPC style
        /// commands on a specified queue.
        /// </summary>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <param name="queueName">The name of the queue used to submit RPC style messages.</param>
        /// <returns>The RPC client assocated with the queue on the specified bus.  If not
        /// found, an exception is thrown.</returns>
        IRpcClient GetRpcClient(string busName, string queueName);
    }
}