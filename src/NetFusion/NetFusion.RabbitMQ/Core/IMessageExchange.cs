using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Components that implement this interface are responsible for creating
    /// exchanges to which messages can be published.
    /// </summary>
    public interface IMessageExchange : IKnownPluginType
    {
        /// <summary>
        /// The broker on which the exchange is to be declared.
        /// </summary>
        string BrokerName { get; }

        /// <summary>
        /// The name of the exchange.
        /// </summary>
        string ExchangeName { get; }

        /// <summary>
        /// The exchange settings used to declare the exchange.
        /// </summary>
        ExchangeSettings Settings { get; }

        /// <summary>
        /// The queues that are to be bound to the exchange.
        /// </summary>
        IEnumerable<ExchangeQueue> Queues { get; }

        /// <summary>
        /// The message type associated with the exchange.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// The optional return type expected from the client that
        /// receives and processes the message.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Called the Message Broker when defining exchanges and allows a 
        /// specific declared application exchange to specific its settings.
        /// </summary>
        void InitializeSettings();

        /// <summary>
        /// Called to declare the exchange and its associated queues on the
        /// message broker.
        /// </summary>
        /// <param name="channel">The channel used to communicate with the
        /// message broker.</param>
        void Declare(IModel channel);

        /// <summary>
        /// Called before a message is published to the exchange to
        /// determine if the event being published meets the criteria
        /// required by the exchange.
        /// </summary>
        /// <param name="message">The message being published.</param>
        /// <returns>Should return True if the event matches the exchange's criteria
        /// and should be posted.  Otherwise, False needs to be returned.</returns>
        bool Matches(IMessage message);

        /// <summary>
        /// Called to publish a message to the exchange.
        /// </summary>
        /// <param name="channel">The channel used to post the message to the queue.</param>
        /// <param name="message">The message to publish to the exchange.</param>
        /// <param name="messageBody">The message serialized based on the content-type and
        /// encoding-type specified by the message attributes.</param>
        void Publish(IModel channel, IMessage message, byte[] messageBody,
            string replyToQueueName = null);
    }
}
