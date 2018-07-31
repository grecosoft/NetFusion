using System;
using NetFusion.Bootstrap.Plugins;
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
        /// An unique value identifying the host application that published
        /// the message.
        /// </summary>
        string PublishingHostId { get; }

        /// <summary>
        /// Determines if a message type has a defined exchange.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>True if there is a defined exchange for the message
        /// type.  Otherwise, False is returned.</returns>
        bool IsExchangeMessage(Type messageType);

        /// <summary>
        /// Returns information about an exchange associated with a
        /// specific message type.
        /// </summary>
        /// <param name="messageType">The type of the mesage.</param>
        /// <returns>The associated exchange defintion.  If not found,
        /// an exception is raised.</returns>
        ExchangeDefinition GetDefinition(Type messageType);

        /// <summary>
        /// Returns the client that should be used to publish a RPC style
        /// message command and correlate the reply from the consumer back
        /// to the orginating command. 
        /// </summary>
        /// <param name="busName">The bus name associated with the client.</param>
        /// <param name="exchangeName">The exchange name associated
        /// with the client.</param>
        /// <returns>Reference to the client.  If a client is not registered 
        /// for the specified exchange type, an exception is raised.</returns>
        IRpcClient GetRpcClient(string busName, string exchangeName);
    }
}