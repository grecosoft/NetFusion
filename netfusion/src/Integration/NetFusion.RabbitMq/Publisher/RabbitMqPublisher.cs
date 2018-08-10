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
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Message publisher implemention that dispatches messages to RabbitMQ having an associated
    /// exchange.  Responsible for creating associated message exchanges and delivering messages
    /// when published. 
    /// https://github.com/grecosoft/NetFusion/wiki/core.messaging.publishers#messaging---publishers
    /// </summary>
    public class RabbitMqPublisher : MessagePublisher,
        IPublisherContext
    {
        // Dependent Services:
        public ILogger Logger { get; }
        public ISerializationManager Serialization { get; }
        public IEntityScriptingService Scripting { get; }
        
        // Dependent Modules:
        public IBusModule BusModule { get; }
        public IPublisherModule PublisherModule { get; }

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
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            // Only process messages for which there are defined exchanges.
            Type messageType = message.GetType();
            if (! PublisherModule.IsExchangeMessage(messageType))
            {
                return;
            }

            // Lookup the exchange associated with the message and the bus
            // on which it should be created.
            ExchangeMeta definition = PublisherModule.GetDefinition(messageType);
            IBus bus = BusModule.GetBus(definition.BusName);

            // Create the exchange/queue:
            CreatedExchange createdExchange = await CreateExchange(bus, definition);

            LogPublishedMessage(createdExchange, message);

            // Publish the message to the created exchange/queue.
            await definition.PublisherStrategy.Publish(this, createdExchange,
                message,
                cancellationToken);
        }

        private async Task<CreatedExchange> CreateExchange(IBus bus, ExchangeMeta meta)
        {
            IExchange exchange = await bus.Advanced.ExchangeDeclareAsync(meta);
            return new CreatedExchange(bus, exchange, meta);
        }

        private void LogPublishedMessage(CreatedExchange exchange, IMessage message)
        {
            Logger.LogTraceDetails(RabbitMqLogEvents.PublisherEvent, 
                "Message being Published to Message Bus.", 
                new {
                    exchange.Meta.BusName,
                    exchange.Meta.ExchangeName,
                    exchange.Meta.QueueMeta?.QueueName,
                    exchange.Meta.ContentType,
                    Message = message
                });
        }
    }
}