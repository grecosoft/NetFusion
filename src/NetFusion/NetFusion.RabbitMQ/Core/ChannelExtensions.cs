using NetFusion.Common;
using NetFusion.Common.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Extension methods for RabbitMQ driver IModel (channel) interface based on
    /// classes used by this plug-in.  This makes the calling code more fluent.
    /// </summary>
    internal static class ChannelExtensions
    {
        public static void ExchangeDeclare(this IModel channel,
            ExchangeSettings settings)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(settings, nameof(settings));

            channel.ExchangeDeclare(settings.ExchangeName, settings.ExchangeType,
                settings.IsDurable,
                settings.IsAutoDelete,
                settings.Arguments);
        }

        public static void QueueDeclare(this IModel channel,
            string queueName,
            QueueSettings settings)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNullOrWhiteSpace(queueName, nameof(queueName));
            Check.NotNull(settings, nameof(settings));

            channel.QueueDeclare(queueName,
                settings.IsDurable,
                settings.IsExclusive,
                settings.IsAutoDelete,
                settings.Arguments);
        }

        public static void QueueDeclare(this IModel channel,
            MessageConsumer eventConsumer)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(eventConsumer, nameof(eventConsumer));

            if (eventConsumer.IsBrokerAssignedName)
            {
                eventConsumer.QueueName = channel.QueueDeclare();
                return;
            }

            channel.QueueDeclare(eventConsumer.QueueName,
                eventConsumer.QueueSettings.IsDurable,
                eventConsumer.QueueSettings.IsExclusive,
                eventConsumer.QueueSettings.IsAutoDelete, null);
        }

        public static void QueueBind(this IModel channel, MessageConsumer eventConsumer)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(eventConsumer, nameof(eventConsumer));

            if (eventConsumer.RouteKeys.Empty())
            {
                channel.QueueBind(eventConsumer.QueueName, eventConsumer.ExchangeName, "");
                return;
            }

            eventConsumer.RouteKeys.ForEach(routeKey => {

                channel.QueueBind(eventConsumer.QueueName, eventConsumer.ExchangeName, routeKey);
            });
        }

        public static void SetBasicConsumer(this IModel channel,
            MessageConsumer eventConsumer)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(eventConsumer, nameof(eventConsumer));

            eventConsumer.Consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(eventConsumer.QueueName, eventConsumer.QueueSettings, eventConsumer.Consumer);
        }

        public static EventingBasicConsumer SetBasicConsumer(this IModel channel, 
            ExchangeQueue queue)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(queue, nameof(queue));

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue.QueueName, queue.Settings, consumer);
            return consumer;
        }

        public static void BasicConsume(this IModel channel,
            string queue,
            QueueSettings settings,
            EventingBasicConsumer basicConsumer)
        {
            Check.NotNullOrWhiteSpace(queue, nameof(queue));
            Check.NotNull(settings, nameof(settings));
            Check.NotNull(basicConsumer, nameof(basicConsumer));

            channel.BasicConsume(queue, settings.IsNoAck, basicConsumer);
        }
    }
}
