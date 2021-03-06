﻿using System;
using System.Collections.Generic;
using NetFusion.Base;
using NetFusion.Messaging.Types.Contracts;
using NetFusion.RabbitMQ.Publisher.Internal;
using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ.Metadata
{
    /// <summary>
    /// Contains metadata describing a broker exchange.
    /// </summary>
    public class ExchangeMeta
    {
        /// <summary>
        /// The name of the configured message bus on which the exchange should be declared.
        /// This value is used to find the bus connection settings within the application's
        /// configuration file.
        /// </summary>
        public string BusName { get; private init; }

        /// <summary>
        /// The RabbitMQ type of the exchange to be created.  Null value represents 
        /// the default exchange. 
        /// </summary>
        public string ExchangeType { get; private init; }
        
        /// <summary>
        /// The name of the exchange.  The name will be null if corresponding to the default exchange.
        /// </summary>
        public string ExchangeName { get; private init; }

        /// <summary>
        /// The type of the message associated with the exchange. 
        /// </summary>
        internal Type MessageType { get; set; }
        
        /// <summary>
        /// If the exchange represents the default exchange, this will reference the queue
        /// metadata that should be created on the default-exchange.
        /// </summary>
        public QueueMeta QueueMeta { get; private set; }

        /// <summary>
        /// Indicates that the queue is on the the RabbitMQ default exchange.
        /// </summary>
        public bool IsDefaultExchange => ExchangeName == null && ExchangeType == null;

        /// <summary>
        /// Responsible for publishing the message based on the exchange type.
        /// </summary>
        internal IPublisherStrategy PublisherStrategy { get; private set; }

        protected ExchangeMeta()
        {
            // Unless otherwise specified, the default publisher strategy will be used.
            PublisherStrategy = new DefaultPublisherStrategy();
        }

        /// <summary>
        /// Defines a queue on the default exchange.
        /// 
        /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-default
        /// </summary>
        /// <typeparam name="TMessage">Type of message associated with exchange.</typeparam>
        /// <param name="busName">The key specified within the application's settings
        /// specifying the broker connection.</param>
        /// <param name="queueName">The name of the queue to be created.</param>
        /// <param name="config">Delegate used to specify the queue metadata.</param>
        /// <returns>The exchange metadata.</returns>
        public static ExchangeMeta<TMessage> DefineDefault<TMessage>(string busName, string queueName,
            Action<QueueMeta> config = null)
            where TMessage : IMessage
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(queueName));

            var exchange = new ExchangeMeta<TMessage> {
                BusName = busName
            };
            
            exchange.QueueMeta = QueueMeta.Define(queueName, exchange);

            config?.Invoke(exchange.QueueMeta);
            return exchange;
        }

        /// <summary>
        /// Defines a queue on the default exchange.
        /// 
        /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-default
        /// </summary>
        /// <param name="busName">The key specified within the application's settings
        /// specifying the broker connection.</param>
        /// <param name="queueName">The name of the queue to be created.</param>
        /// <param name="config">Delegate used to specify the queue metadata.</param>
        /// <returns>The exchange metadata.</returns>
        public static ExchangeMeta DefineDefault(string busName, string queueName,
            Action<QueueMeta> config = null)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(queueName));

            var exchange = new ExchangeMeta
            {
                BusName = busName
            };

            exchange.QueueMeta = QueueMeta.Define(queueName, exchange);

            config?.Invoke(exchange.QueueMeta);
            return exchange;
        }

        /// <summary>
        /// Defines an exchange.
        /// </summary>
        /// <typeparam name="TMessage">Type of message associated with exchange.</typeparam>
        /// <param name="busName">The key specified within the application's settings
        /// specifying the broker connection.</param>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="exchangeType">The RabbitMQ exchange type specifier.</param>
        /// <param name="config">Delegate used to specify additional exchange metadata.</param>
        /// <returns>The exchange metadata.</returns>
        public static ExchangeMeta<TMessage> Define<TMessage>(string busName, string exchangeName, 
            string exchangeType,
            Action<ExchangeMeta> config = null)
            where TMessage : IMessage
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name not specified.", nameof(exchangeName));
            if (string.IsNullOrWhiteSpace(exchangeType))
                throw new ArgumentException("Exchange type not specified", nameof(exchangeType));

            var exchange = new ExchangeMeta<TMessage>
            {
                BusName = busName,
                ExchangeName = exchangeName,
                ExchangeType = exchangeType
            };

            config?.Invoke(exchange);
            return exchange;
        }

        /// <summary>
        /// Defines an exchange.
        /// </summary>
        /// <param name="busName">The key specified within the application's settings
        /// specifying the broker connection.</param>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="exchangeType">The RabbitMQ exchange type specifier.</param>
        /// <param name="config">Delegate used to specify additional exchange metadata.</param>
        public static ExchangeMeta Define(string busName, string exchangeName, string exchangeType,
            Action<ExchangeMeta> config = null)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name not specified.", nameof(exchangeName));
            if (string.IsNullOrWhiteSpace(exchangeType))
                throw new ArgumentException("Exchange type not specified", nameof(exchangeType));

            var exchange = new ExchangeMeta
            {
                BusName = busName,
                ExchangeName = exchangeName,
                ExchangeType = exchangeType
            };

            config?.Invoke(exchange);
            return exchange;
        }

        /// <summary>
        /// Sets a specific publisher strategy to be used.
        /// </summary>
        /// <param name="strategy">Reference to strategy.</param>
        internal void SetPublisherStrategy(IPublisherStrategy strategy)
        {
            PublisherStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Determines if the message meets the criteria to be published to the exchange.
        /// </summary>
        /// <param name="message">The message being published.</param>
        /// <returns>True if the message should be published to the exchange.  Otherwise, False.</returns>
        internal virtual bool Applies(IMessage message) => true;

        /// <summary>
        /// Survive server restarts. If this parameter is false, the exchange
        /// will be removed when the server restarts.
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// Delete this exchange when the last queue is unbound.
        /// </summary>
        public bool IsAutoDelete { get; set; }

        /// <summary>
        /// Tells RabbitMQ to save the message to disk or to cache. 
        /// </summary>
        public bool IsPersistent { get; set; }

        /// <summary>
        /// The optional route key associated with the exchange definition.  If specified,
        /// the route-key will be set on all messages delivered to the exchanged.
        /// </summary>
        internal string RouteKey { get; set; }

        /// <summary>
        /// Indicates that the definition is for a RPC style message.
        /// </summary>
        internal bool IsRpcExchange { get; set; }

        /// <summary>
        /// Applies exchange settings specified within the application's configuration
        /// to the exchange metadata.  Only values specified are set.
        /// </summary>
        /// <param name="settings">External stored exchange settings.</param>
        internal void ApplyOverrides(ExchangeSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            ContentType = settings.ContentType ?? ContentType;
            CancelRpcRequestAfterMs = settings.CancelRpcRequestAfterMs ?? CancelRpcRequestAfterMs;
            IsNonRoutedSaved = settings.IsNonRoutedSaved ?? IsNonRoutedSaved;
        }

        /// <summary>
        /// Indicates messages published to the queue are of importance
        /// and should be written to the undelivered message exchange.
        /// </summary>
        public bool IsNonRoutedSaved { get; set; }

        /// <summary>
        /// The content type of the serialized message body.
        /// </summary>
        public string ContentType { get; set; } = ContentTypes.Json;

        /// <summary>
        /// Number of milliseconds after which a RPC request will timeout
        /// if a response is not received from the message consumer.
        /// </summary>
        public int CancelRpcRequestAfterMs { get; set; } = SettingDefaults.RpcTimeOutAfterMs;

        /// <summary>
        /// Based on the type of exchange, used to identify a namespace to the
        /// consumer processing the message.  
        /// </summary>
        public string ActionNamespace { get; set; }

        /// <summary>
        /// Adds exchange log information to the dictionary of values.
        /// </summary>
        /// <param name="log">Dictionary containing log values.</param>
        internal void LogProperties(IDictionary<string, object> log)
        {
            log["Exchange"] = IsDefaultExchange ? "Default-Exchange" : GetLogDetails();
            if (QueueMeta != null)
            {
                log["Queue"] = QueueMeta.GetLogDetails();
            }
        }

        /// <summary>
        /// Returns an anonymous type containing the exchange properties to be logged.
        /// </summary>
        /// <returns>Object with properties to be logged.</returns>
        internal object GetLogDetails()
        {
            return new
            {
                BusName,
                ExchangeName,
                ExchangeType,
                MessageType = MessageType?.FullName ?? "n/a",
                IsAutoDelete,
                IsDurable,
                IsPersistent,
                IsRpcExchange,
                IsDefaultExchange,
                RouteKey,
                ContentType,
                ActionNamespace = ActionNamespace ?? "n/a",
                CancelRpcRequestAfterMs = IsRpcExchange ? CancelRpcRequestAfterMs.ToString() : "n/a"
            };
        }
    }
    
    /// <summary>
    /// ExchangeMeta derived class for a specified type of message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class ExchangeMeta<TMessage> : ExchangeMeta
        where TMessage : IMessage
    {
        public ExchangeMeta()
        {
            MessageType = typeof(TMessage);
        }

        /// <summary>
        /// Overrides the base implementation that invokes a message
        /// type specific delegate determining if the message applies.
        /// </summary>
        /// <param name="message">The message being published to exchange.</param>
        /// <returns>True if message applies.  Otherwise, false.</returns>
        internal override bool Applies(IMessage message)
        {
            return AppliesIf((TMessage)message);
        }

        /// <summary>
        /// Delegate that can be specified within code to determine if the 
        /// state of the message applied to the exchange and should be published.
        /// </summary>
        /// <returns>True if message applies.  Otherwise, false.</returns>
        public Func<TMessage, bool> AppliesIf { get; set; } = m => true;
    }
}