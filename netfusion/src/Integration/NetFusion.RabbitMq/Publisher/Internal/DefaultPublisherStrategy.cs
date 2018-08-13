using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ;
using System;
using System.Threading;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// The default strategy containing the implementation used to publish the
    /// majority of the message exchange types.
    /// </summary>
    internal class DefaultPublisherStrategy : IPublisherStrategy
    {
        public async Task Publish(IPublisherContext context, CreatedExchange createdExchange, 
            IMessage message,
            CancellationToken cancellationToken)
        {
            // Prepare the message and its defining properties to be published.
            byte[] messageBody = context.Serialization.Serialize(message, createdExchange.Meta.ContentType);
            MessageProperties messageProperties = GetMessageProperties(context, createdExchange, message);
            string routeKey = message.GetRouteKey() ?? createdExchange.Meta.RouteKey;

            // Only send the message to the exchange if satisfying all constraints
            // configured for the exchange.
            if (! await Satisfies(context, createdExchange, message))
            {
                return;
            }

            // Delegate to EasyNetQ to publish the message.
            await createdExchange.Bus.Advanced.PublishAsync(createdExchange.Exchange,
                routeKey ?? string.Empty, 
                false,
                messageProperties,
                messageBody);
        }
        
        public static MessageProperties GetMessageProperties(IPublisherContext context, 
            CreatedExchange createdExchange, 
            IMessage message)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (createdExchange == null) throw new ArgumentNullException(nameof(createdExchange));
            if (message == null) throw new ArgumentNullException(nameof(message));

            var definition = createdExchange.Meta;
       
            var props = new MessageProperties 
            {
                ContentType = definition.ContentType,
                AppId = context.BusModule.HostAppId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            if (definition.IsPersistent)
            {
                props.DeliveryMode = 2; 
            }

            string correlationId = message.GetCorrelationId();
            if (correlationId != null)
            {
                props.CorrelationId = correlationId;
            }

            byte? msgPriority = message.GetPriority();
            if (msgPriority != null)
            {
                props.Priority = msgPriority.Value;
            }

            return props;
        }

       private static async Task<bool> Satisfies(IPublisherContext context, CreatedExchange createdExchange, IMessage message)
       {
           ExchangeMeta definition = createdExchange.Meta;
           bool applies = definition.Applies(message);

           if (applies && definition.ScriptPredicate != null)
           {
               return await context.Scripting.SatisfiesPredicateAsync(message, definition.ScriptPredicate);
           }

           return applies;
        }
    }
}