using NetFusion.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Associates a channel and the basic consumer that was created
    /// for it to handle the receiving of a given message.  A given
    /// message consumer has a collection of these object.  By default,
    /// there will be only one consumer message-handler.  But this can
    /// be controlled via the broker configuration file for frequently
    /// published messages so they can be handled by multiple consumer
    /// threads.
    /// </summary>
    public class MessageHandler
    {
        public IModel Channel { get; private set; }
        public EventingBasicConsumer EventConsumer { get; private set; }

        public MessageHandler(IModel channel, EventingBasicConsumer eventConsumer)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(eventConsumer, nameof(eventConsumer));

            this.Channel = channel;
            this.EventConsumer = eventConsumer;
        }
    }
}
