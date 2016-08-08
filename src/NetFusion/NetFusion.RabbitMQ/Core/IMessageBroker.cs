using NetFusion.Messaging;
using NetFusion.RabbitMQ.Integration;
using NetFusion.RabbitMQ.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Interface exposed by the message broker providing
    /// only those methods that external components can use.
    /// </summary>
    public interface IMessageBroker
    {
        /// <summary>
        /// Initializes the message broker with the information needed to create 
        /// exchanges and queues.
        /// </summary>
        /// <param name="metadata">The exchange and queue meta-data.</param>
        void Initialize(MessageBrokerMetadata metadata);

        /// <summary>
        /// Creates the needed RabbitMq exchanges based on the configurations.
        /// </summary>
        void DefineExchanges();

        /// <summary>
        /// Based on consumers having handlers for messages associated with an 
        /// exchange, invoke the corresponding handler when an message is delivered.
        /// </summary>
        /// <param name="messageConsumers">List of discovered exchange message consumers.</param>
        void BindConsumers(IEnumerable<MessageConsumer> messageConsumers);

        /// <summary>
        /// Determines if a message has an associated exchange.
        /// </summary>
        /// <param name="message">The type of the message.</param>
        /// <returns>True if there is an exchange defined for the 
        /// message.  Otherwise, False.</returns>
        bool IsExchangeMessage(IMessage message);

        /// <summary>
        /// Publishes a message to an exchange.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        void PublishToExchange(IMessage message);

        void AddSerializer(IMessageSerializer serializer);

        void SetExchangeMetadataReader(Func<string, Task<IEnumerable<ExchangeConfig>>> reader);
    }
}
