using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for declaring and publishing 
    /// messages to exchanges.
    /// </summary>
    public class ExchangePublisherSetup
    {
        private IContainerLogger _logger;
        private MessageBrokerConfig _brokerConfig;
        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;
        private readonly IEntityScriptingService _scriptingSrv;

        private ILookup<Type, ExchangeMessageDefinition> _messageExchanges;

        public ExchangePublisherSetup(
            IContainerLogger logger,
            MessageBrokerConfig brokerConfig,
            IConnectionManager connectionManager,
            ISerializationManager serializationManger,
            IEntityScriptingService scriptiongSrv)
        {
            _logger = logger.ForPluginContext<ExchangePublisherSetup>();
            _brokerConfig = brokerConfig;
            _connMgr = connectionManager;
            _serializationMgr = serializationManger;
            _scriptingSrv = scriptiongSrv;

            // Messages can have one or more associated exchanges.
            _messageExchanges = brokerConfig.Exchanges.ToLookup(
                k => k.MessageType,
                e => new ExchangeMessageDefinition(e, e.MessageType));
        }

        /// <summary>
        /// Creates the exchanges and associated queues defined in the running
        /// application to which messages can be published.
        /// </summary>
        public void DeclareExchanges()
        {
            ExchangeMessageDefinition[] exchangeDefs = GetExchangeDefinitions();
            foreach (ExchangeMessageDefinition exDef in exchangeDefs)
            {
                using (IModel channel = _connMgr.CreateChannel(exDef.Exchange.BrokerName))
                {
                    exDef.Exchange.Declare(channel);
                }
            }
        }

        private ExchangeMessageDefinition[] GetExchangeDefinitions()
        {
            return _messageExchanges.Values().ToArray();
        }

        /// <summary>
        /// Determines if the specified message has an associated exchange
        /// to which it can be published.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if one or more exchanges are associated with the message.</returns>
        public bool IsExchangeMessage(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            return _messageExchanges.Contains(message.GetType());
        }

        /// <summary>
        /// Publishes the message to all exchanges associated with 
        /// the message's type.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>The future result.</returns>
        public async Task PublishToExchange(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            Type messageType = message.GetType();
            IEnumerable<ExchangeMessageDefinition> exchangeDefs = _messageExchanges[messageType];

            if (exchangeDefs == null)
            {
                throw new BrokerException(
                    $"The message of type: {messageType.FullName} is not associated with an exchange.");
            }

            foreach (ExchangeMessageDefinition exchangeDef in exchangeDefs)
            {
                await Publish(exchangeDef, message);
            }
        }

        private async Task Publish(ExchangeMessageDefinition exchangeDef, IMessage message)
        {
            if (!await MatchesExchangeCriteria(exchangeDef, message)) return;

            IMessageExchange exchange = exchangeDef.Exchange;

            string[] orderedContentTypes = new string[] {
                message.GetContentType(),
                exchange.Settings.ContentType};

            LogPublishingExchangeMessage(message, exchangeDef);

            byte[] messageBody = _serializationMgr.Serialize(message, orderedContentTypes);

            using (var channel = _connMgr.CreateChannel(exchange.BrokerName))
            {
                exchangeDef.Exchange.Publish(channel, message, messageBody);
            }
        }

        // Determines if the message should be delivered to the queue.  If the exchange is marked
        // with a predicate attribute, the corresponding externally named script is executed to 
        // determine if the message has passing criteria.  If no external script is specified,
        // the exchange's matches method is called.
        private async Task<bool> MatchesExchangeCriteria(ExchangeMessageDefinition exchangeDef, IMessage message)
        {
            ScriptPredicate predicate = exchangeDef.Exchange.Settings.Predicate;

            if (predicate != null)
            {
                return await _scriptingSrv.SatifiesPredicate(message, predicate);
            }

            return exchangeDef.Exchange.Matches(message);
        }

        private void LogPublishingExchangeMessage(IMessage message,
           ExchangeMessageDefinition exchangeDef)
        {
            _logger.Verbose("Publishing to Exchange", () =>
            {
                return new
                {
                    Message = message,
                    ContentType = message.GetContentType(),
                    exchangeDef.Exchange.BrokerName,
                    exchangeDef.Exchange.ExchangeName
                };
            });
        }
    }
}
