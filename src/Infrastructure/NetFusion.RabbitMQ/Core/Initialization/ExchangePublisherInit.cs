﻿using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Types;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for declaring and publishing messages to exchanges.  This initialization
    /// class is from the publisher's perspective.  The exchange and queue definition classes found
    /// during the bootstrap process are used to create the corresponding exchanges and queues on the
    /// RabbitMQ broker.  There is no attempt to delete an existing exchange and the default RabbitMQ
    /// behavior is used.  If any of the exchange or queue settings change, RabbitMQ will return and
    /// exception.  This is the safest approach.
    /// </summary>
    public class ExchangePublisherInit : IBrokerInitializer
    {
        private ILogger _logger;
        private MessageBrokerState _brokerState;
        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;
        private readonly IEntityScriptingService _scriptingSrv;

        private ILookup<Type, MessageExchangeDefinition> _messageExchanges;

        public ExchangePublisherInit(
            ILoggerFactory loggerFactory,
            MessageBrokerState brokerState,
            IEntityScriptingService scriptiongSrv)
        {
            _logger = loggerFactory.CreateLogger<ExchangePublisherInit>();
            _brokerState = brokerState;
            _connMgr = brokerState.ConnectionMgr;
            _serializationMgr = brokerState.SerializationMgr;
            _scriptingSrv = scriptiongSrv;

            // Messages can have one or more associated exchanges.  When a message is published, 
            // a lookup is completed to determine if there is an exchange associated with the message.
            _messageExchanges = brokerState.Exchanges.ToLookup(
                k => k.MessageType,
                e => new MessageExchangeDefinition(e, e.MessageType));
        }

        /// <summary>
        /// Creates the exchanges and associated queues defined in the running
        /// application to which messages can be published.
        /// </summary>
        public void DeclareExchanges()
        {
            MessageExchangeDefinition[] exchangeDefs = GetExchangeDefinitions();
            foreach (MessageExchangeDefinition exDef in exchangeDefs)
            {
                using (IModel channel = _connMgr.CreateChannel(exDef.Exchange.BrokerName))
                {
                    exDef.Exchange.Declare(channel);
                }
            }
        }

        public void LogDetails(IDictionary<string, object> log)
        {

        }

        private MessageExchangeDefinition[] GetExchangeDefinitions()
        {
            return _messageExchanges.Values().ToArray();
        }

        /// <summary>
        /// Determines if the specified message has an associated exchange to which it can be published.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if one or more exchanges are associated with the message.</returns>
        public bool IsExchangeMessage(IMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return _messageExchanges.Contains(message.GetType());
        }

        /// <summary>
        /// Publishes the message to all exchanges associated with the message's type.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>The future result.</returns>
        public async Task PublishToExchangeAsync(IMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Type messageType = message.GetType();
            IEnumerable<MessageExchangeDefinition> exchangeDefs = _messageExchanges[messageType];

            if (exchangeDefs == null)
            {
                throw new BrokerException(
                    $"The message of type: {messageType.FullName} is not associated with an exchange.");
            }

            foreach (MessageExchangeDefinition exchangeDef in exchangeDefs)
            {
                await PublishAsync(exchangeDef, message);
            }
        }

        private async Task PublishAsync(MessageExchangeDefinition exchangeDef, IMessage message)
        {
            if (!await SatisfiesExchangeCriteria(exchangeDef, message)) return;

            IMessageExchange exchange = exchangeDef.Exchange;

            string[] orderedContentTypes = {
                message.GetContentType(),
                exchange.Settings.ContentType};

            byte[] messageBody = _serializationMgr.Serialize(message, orderedContentTypes);

            LogPublishingExchangeMessage(message, exchangeDef);

            using (var channel = _connMgr.CreateChannel(exchange.BrokerName))
            {
                exchangeDef.Exchange.Publish(channel, message, messageBody);
            }
        }

        // Determines if the message should be delivered to the queue.  If the exchange is marked
        // with a predicate attribute, the corresponding externally named script is executed to 
        // determine if the message has passing criteria.  If no external script is specified,
        // the exchange's Satisfies method is called.
        private async Task<bool> SatisfiesExchangeCriteria(MessageExchangeDefinition exchangeDef, IMessage message)
        {
            ScriptPredicate predicate = exchangeDef.Exchange.Settings.Predicate;

            if (predicate != null)
            {
                return await _scriptingSrv.SatisfiesPredicateAsync(message, predicate);
            }

            return exchangeDef.Exchange.Satisfies(message);
        }

        private void LogPublishingExchangeMessage(IMessage message,
           MessageExchangeDefinition exchangeDef)
        {
            _logger.LogTraceDetails(RabbitMqLogEvents.MESSAGE_PUBLISHER, "Publishing to Exchange", 
                new
                {
                    Message = message,
                    ContentType = message.GetContentType(),
                    exchangeDef.Exchange.BrokerName,
                    exchangeDef.Exchange.ExchangeName
                });
        }
    }
}
