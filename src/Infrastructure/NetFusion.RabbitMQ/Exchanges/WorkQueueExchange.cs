using NetFusion.Common.Extensions;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;
using System;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Represents a work queue.  This delivery pattern is based on the default exchange.
    /// If a message is delivered to a queue with multiple consumers, the consumers will
    /// be called round-robin.  Either the publisher or the consumer can define the queues.
    /// The published messages will be delivered to the queue with the name matching the
    /// route-key of the published message.
    /// </summary>
    public abstract class WorkQueueExchange<TMessage> : MessageExchange<TMessage>
        where TMessage : IMessage
    {
        public WorkQueueExchange()
        {
            Settings.ExchangeType = null;      // Default Exchange
            Settings.ExchangeName = "";     
             
            QueueSettings.IsDurable = true;
            QueueSettings.IsNoAck = false;   // Require consumer to acknowledge message.
            QueueSettings.IsAutoDelete = false;
            QueueSettings.IsExclusive = false;
        }

        // For work-queue style of exchanges, the route key on the published message
        // is used to determine the queue to which it should be sent.  Since the route
        // key on messages are upper-cased, the queue name will be also upper-cased.
        protected override void OnApplyConventions()
        {
           foreach (ExchangeQueue queue in this.Queues)
            {
                queue.QueueName = queue.QueueName.ToUpper();
            }
        }

        internal override void ValidateConfiguration()
        {
            base.ValidateConfiguration();
            if (!Settings.ExchangeName.IsNullOrEmpty())
            {
                throw new InvalidOperationException(
                    $"Exchange Name cannot be set for a WorkQueue.  Exchange Type: {GetType()}.");
            }
        }

        protected override void OnSetPublisherBasicProperties(IModel channel, IMessage message, IBasicProperties properties)
        {
            // Make the message persistent.
            properties.Persistent = true;
        }
    }
}
