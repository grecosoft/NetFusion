using NetFusion.Common;
using System;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Information on how a message maps to an exchange.
    /// </summary>
    internal class MessageExchangeDefinition
    {
        /// <summary>
        /// The message type associated with the exchange.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// Details on how the exchange should be declared and information
        /// for the queues that should be created.
        /// </summary>
        public IMessageExchange Exchange { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exchange">Details on how the exchange should be declared and information
        /// for the queues that should be created.</param>
        /// <param name="messageType"> The message type associated with the exchange.</param>
        public MessageExchangeDefinition(IMessageExchange exchange, Type messageType = null)
        {
            Check.NotNull(exchange, nameof(exchange));

            this.MessageType = messageType;
            this.Exchange = exchange;
        }
    }
} 
