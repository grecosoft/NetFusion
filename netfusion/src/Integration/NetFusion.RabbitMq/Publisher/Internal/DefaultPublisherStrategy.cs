using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ;
using System;
using System.Threading;
using NetFusion.Messaging.Types;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// The default strategy containing the implementation used to publish the
    /// majority of the message types.
    /// </summary>
    internal class DefaultPublisherStrategy : IPublisherStrategy
    {
        public async Task Publish(IPublisherContext context, CreatedExchange createdExchange, 
            IMessage message,
            CancellationToken cancellationToken)
        {
            byte[] messageBody = context.Serialization.Serialize(message, createdExchange.Definition.ContentType);
            MessageProperties messageProperties = GetMessageProperties(context, createdExchange, message);
            string routeKey = message.GetRouteKey() ?? createdExchange.Definition.RouteKey;

            // Only send the message to the exchange if satifying all constraints
            // configured for the exchange.
            if (! await Satisfies(context, createdExchange, message))
            {
                return;
            }

            await createdExchange.Bus.Advanced.PublishAsync(createdExchange.Exchange,
                routeKey, 
                false,
                messageProperties,
                messageBody);
        }

        public static MessageProperties GetMessageProperties(IPublisherContext context, 
            CreatedExchange createdExchange, 
            IMessage message)
        {
            var definition = createdExchange.Definition;
       
            var props = new MessageProperties 
            {
                ContentType = definition.ContentType,
                AppId = context.PublisherModule.PublishingHostId,
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
            ExchangeDefinition definition = createdExchange.Definition;
            bool applies = definition.DelegatePredicate(message);

            if (applies && definition.ScriptPredicate != null)
            {
                return await context.Scripting.SatisfiesPredicateAsync(message, definition.ScriptPredicate);
            }

            return applies;
        }
    }
}