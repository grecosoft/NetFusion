using NetFusion.Common.Extensions;
using NetFusion.Messaging;
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

        internal override void ValidateConfiguration()
        {
            base.ValidateConfiguration();
            if (!Settings.ExchangeName.IsNullOrEmpty())
            {
                throw new InvalidOperationException(
                    $"Exchange Name cannot be set for a WorkQueue.  Exchange Type: {GetType()}.");
            }
        }

        public override IBasicProperties GetBasicProperties(IModel channel, IMessage message)
        {
            // Only deliver message to consumer if no pending acknowledgments.
            channel.BasicQos(0, 1, false);

            // Make the message persistent.
            var basicProps = base.GetBasicProperties(channel, message);
            basicProps.Persistent = true;
            return basicProps;
        }
    }
}
