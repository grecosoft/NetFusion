using IMessage = NetFusion.Messaging.Types.IMessage;
using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Base.Scripting;
using NetFusion.Messaging.Core;
using NetFusion.RabbitMQ.Modules;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.RabbitMQ.Publisher.Internal;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Message publisher implemention that dispatches messages to RabbitMQ having an associated
    /// exchange.  Responsible for creating associated message exchanges and delivering messages. 
    /// </summary>
    public class RabbitMqPublisher : MessagePublisher,
        IPublisherContext
    {
        public ILogger Logger { get; }
        public IBusModule BusModule { get; }
        public IPublisherModule PublisherModule { get; }
        public ISerializationManager Serialization { get; }
        public IEntityScriptingService Scripting { get; }

        public RabbitMqPublisher(ILoggerFactory loggerFactory,
            IBusModule busModule, 
            IPublisherModule publisherModule,
            ISerializationManager serializationManager,
            IEntityScriptingService scripting)
        {
            Logger = loggerFactory.CreateLogger<RabbitAdvancedBus>();
            BusModule = busModule ?? throw new ArgumentNullException(nameof(busModule));    
            PublisherModule = publisherModule ?? throw new ArgumentNullException(nameof(publisherModule));
            Serialization = serializationManager ?? throw new ArgumentNullException(nameof(serializationManager));
            Scripting = scripting ?? throw new ArgumentNullException(nameof(scripting));
        }

        // Indicate that this publisher will deliver messages outside of the running process.
        // This can be used by the caller if they want to publish in-process messages before
        // publishing any external integration messages.
        public override IntegrationTypes IntegrationType => IntegrationTypes.External;

        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Only process messages for which there are defined exchanges.
            Type messageType = message.GetType();
            if (! PublisherModule.IsExchangeMessage(messageType))
            {
                return;
            }

            ExchangeDefinition definition = PublisherModule.GetDefinition(messageType);
            IBus bus = BusModule.GetBus(definition.BusName);
            CreatedExchange createdExchange = await CreateExchange(bus, definition);

            LogPublishedMessage(createdExchange, message);

            await definition.PublisherStrategy.Publish(this, createdExchange,
                message,
                cancellationToken);
        }

        private async Task<CreatedExchange> CreateExchange(IBus bus, ExchangeDefinition definition)
        {
            // Determine if this is a queue that should be created on the default exchange.
            if (definition.IsDefaultExchangeQueue)
            {
                return await CreateQueueOnDefaultExchange(bus, definition);
            }

            // Otherwise, create the exchange to which messages can be published.
            IExchange exchange = await bus.Advanced.ExchangeDeclareAsync(definition.ExchangeName, definition.ExchangeType, 
                durable: definition.IsDurable,
                autoDelete: definition.IsAutoDelete, 
                passive: definition.IsPassive,
                alternateExchange: definition.AlternateExchangeName);

            return new CreatedExchange(bus, exchange, definition);
        }

        private async Task<CreatedExchange> CreateQueueOnDefaultExchange(IBus bus, ExchangeDefinition definition)
        {
            QueueSettings configuredSettings = BusModule.GetQueueSettings(definition.BusName, definition.QueueName);
            if (configuredSettings != null)
            {
                await bus.Advanced.QueueDeclareAsync(definition.QueueName,
                    configuredSettings.Passive,
                    perQueueMessageTtl: configuredSettings.PerQueueMessageTtl,
                    deadLetterExchange: configuredSettings.DeadLetterExchange,
                    deadLetterRoutingKey: configuredSettings.DeadLetterRoutingKey,
                    maxPriority: configuredSettings.MaxPriority);        
            }
            else
            {
                // Use all default conventions"
                await bus.Advanced.QueueDeclareAsync(definition.QueueName);
            }

            return new CreatedExchange(bus, Exchange.GetDefault(), definition);
        }

        private void LogPublishedMessage(CreatedExchange exchange, IMessage message)
        {
            Logger.LogTraceDetails(RabbitMqLogEvents.PublisherEvent, 
                "Message being Published to Message Bus.", 
                new {
                    exchange.Definition.BusName,
                    exchange.Definition.ExchangeName,
                    exchange.Definition.QueueName,
                    exchange.Definition.ContentType,
                    Message = message
                });
        }
    }
}