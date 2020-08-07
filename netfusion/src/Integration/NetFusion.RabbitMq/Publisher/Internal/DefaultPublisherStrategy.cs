using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;
using System.Threading.Tasks;
using EasyNetQ;
using System;
using System.Threading;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

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
            byte[] messageBody = context.Serialization.Serialize(message, createdExchange.Definition.ContentType);
            MessageProperties messageProperties = GetMessageProperties(context, createdExchange, message);
            string routeKey = message.GetRouteKey() ?? createdExchange.Definition.RouteKey;

            // Only send the message to the exchange if satisfying all constraints configured for the exchange.  
            // This only applies to domain-events, as commands are more specific in that they are explicitly
            // requesting a behavior to be taken. 
            if (message is IDomainEvent && ! createdExchange.Definition.Applies(message))
            {
                return;
            }

            // Delegate to EasyNetQ to publish the message.
            await createdExchange.Bus.Advanced.PublishAsync(createdExchange.Exchange,
                routeKey ?? string.Empty, 
                false,
                messageProperties,
                messageBody).ConfigureAwait(false);
        }
        
        public static MessageProperties GetMessageProperties(IPublisherContext context, 
            CreatedExchange createdExchange, 
            IMessage message)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (createdExchange == null) throw new ArgumentNullException(nameof(createdExchange));
            if (message == null) throw new ArgumentNullException(nameof(message));

            var definition = createdExchange.Definition;

            var props = new MessageProperties
            {
                ContentType = definition.ContentType,
                AppId = context.BusModule.HostAppId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DeliveryMode = (byte) (definition.IsPersistent ? 2 : 1)
            };
            
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
    }
}