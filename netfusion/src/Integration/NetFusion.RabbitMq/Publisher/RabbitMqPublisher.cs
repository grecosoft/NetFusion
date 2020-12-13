using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;
using System;
using System.Threading;
using System.Threading.Tasks;
using NetFusion.RabbitMQ.Publisher.Internal;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Logging;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Plugin;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Message publisher implementation that dispatches messages to RabbitMQ having an associated
    /// exchange.  Responsible for creating associated message exchanges and delivering messages
    /// when published. 
    /// </summary>
    public class RabbitMqPublisher : MessagePublisher,
        IPublisherContext
    {
        // Dependent Services:
        public ILoggerFactory LoggerFactory { get; }
        public ISerializationManager Serialization { get; }

        // Dependent Modules:
        public IBusModule BusModule { get; }
        public IPublisherModule PublisherModule { get; }
        
        private readonly IMessageLogger _messageLogger;

        public RabbitMqPublisher(ILoggerFactory loggerFactory,
            IBusModule busModule, 
            IPublisherModule publisherModule,
            ISerializationManager serializationManager,
            IMessageLogger messageLogger)
        {
            LoggerFactory = loggerFactory;
            BusModule = busModule ?? throw new ArgumentNullException(nameof(busModule));    
            PublisherModule = publisherModule ?? throw new ArgumentNullException(nameof(publisherModule));
            Serialization = serializationManager ?? throw new ArgumentNullException(nameof(serializationManager));

            _messageLogger = messageLogger ?? throw new ArgumentNullException(nameof(messageLogger));
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

            var msgLog = new MessageLog(message, LogContextType.PublishedMessage);
            msgLog.SentHint("publish-rabbitmq");
            
            // Lookup the exchange associated with the message to which
            // the message should be published.
            CreatedExchange exchange = PublisherModule.GetExchange(messageType);

            LogMessageExchange(exchange.Meta);
            AddExchangeMetaToLog(msgLog, exchange.Meta);

            // Publish the message to the created exchange/queue.
            await exchange.Meta.PublisherStrategy.Publish(this, exchange,
                message,
                cancellationToken);

            await _messageLogger.LogAsync(msgLog);
        }

        // ---------------------------------- [Logging] ----------------------------------

        private void LogMessageExchange(ExchangeMeta exchangeMeta)
        {
            var logger = LoggerFactory.CreateLogger<RabbitMqPublisher>();
            
            var exchangeInfo = new {
                exchangeMeta.MessageType,
                exchangeMeta.BusName,
                exchangeMeta.ExchangeName,
                exchangeMeta.QueueMeta?.QueueName,
                exchangeMeta.ContentType
            };

            var log = LogMessage.For(LogLevel.Debug, "Publishing {MessageType} to {Exchange} on {Bus}", 
                exchangeInfo.MessageType,
                exchangeInfo.ExchangeName, 
                exchangeInfo.BusName).WithProperties(
                    new LogProperty { Name = "ExchangeInfo", Value = exchangeInfo });
            
            logger.Log(log);
        }
        
        private void AddExchangeMetaToLog(MessageLog msgLog, ExchangeMeta definition)
        {
            if (!_messageLogger.IsLoggingEnabled) return;
            
            if (definition.IsDefaultExchange)
            {
                msgLog.AddLogDetail("Exchange Type", "Default");
                msgLog.AddLogDetail("Queue Name", definition.QueueMeta.QueueName);
            }
            else
            {
                msgLog.AddLogDetail("Exchange Name", definition.ExchangeName);
                msgLog.AddLogDetail("Exchange Type", definition.ExchangeType);
                msgLog.AddLogDetail("Route Key", definition.RouteKey);
                msgLog.AddLogDetail("Content Type", definition.ContentType);
            
                msgLog.AddLogDetail("Is Durable", definition.IsDurable.ToString());
                msgLog.AddLogDetail("Is Auto Delete", definition.IsAutoDelete.ToString());
                msgLog.AddLogDetail("Is Persistent", definition.IsPersistent.ToString());
            }
        }
    }
}