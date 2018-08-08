using System;
using System.Collections.Generic;
using NetFusion.Base;
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
        public string BusName { get; private set; }

        /// <summary>
        /// The RabbitMQ type of the exchange to be created.  Null value represents 
        /// the default exchange. 
        /// </summary>
        public string ExchangeType { get; private set; }

        /// <summary>
        /// The type of the message associated with the exchange. 
        /// </summary>
        internal Type MessageType { get; private set; }

        /// <summary>
        /// The name of the exchange.  The name will be null if corresponding to the default exchange.
        /// </summary>
        public string ExchangeName { get; private set; }

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

        private ExchangeMeta()
        {
            // Unless otherwise specified, the default publisher strategy will be used.
            PublisherStrategy = new DefaultPublisherStrategy();
        }

        /// <summary>
        /// Defines a queue on the default exchange.
        /// </summary>
        /// <param name="busName">The key specified within the application's settings
        /// specifying the broker connection.</param>
        /// <param name="queueName">The name of the queue to be created.</param>
        /// <param name="messageType">The message type assocated with queue.</param>
        /// <param name="config">Delegate used to specify the queue metadata.</param>
        /// <returns>The exchange metadata.</returns>
        public static ExchangeMeta DefineDefault(string busName, string queueName, Type messageType,
            Action<QueueMeta> config = null)
        {
            var exchange = DefineDefault(busName, queueName, config);

            exchange.MessageType = messageType;
            return exchange;
        }

        /// <summary>
        /// Defines a queue on the default exchange.
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
        /// <param name="busName">The key specified within the application's settings
        /// specifying the broker connection.</param>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="exchangeType">The RabbitMQ exchange type specifier.</param>
        /// <param name="messageType">The message type assocated with the exchange.</param>
        /// <param name="config">Delegate used to specify additional exchange metadata.</param>
        /// <returns>The exchange metadata.</returns>
        public static ExchangeMeta Define(string busName, string exchangeName, string exchangeType, Type messageType,
            Action<ExchangeMeta> config = null)
        {
            var exchange = Define(busName, exchangeName, exchangeType, config);
            exchange.MessageType = messageType;

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
        /// Set a specific publisher strategy to be used.
        /// </summary>
        /// <param name="strategy">Reference to strategy.</param>
        internal void SetPublisherStrategy(IPublisherStrategy strategy)
        {
            PublisherStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Survive server restarts. If this parameter is false, the exchange
        /// will be removed when the server restarts.
        /// </summary>
        internal bool IsDurable { get; set; }

        /// <summary>
        /// Delete this exchange when the last queue is unbound.
        /// </summary>
        internal bool IsAutoDelete { get; set; }

        /// <summary>
        /// Tells RabbitMQ to save the message to disk or to cache. 
        /// </summary>
        internal bool IsPersistent { get; set; }

        /// <summary>
        /// Indicates that an exception should be thrown if the exchange
        /// has not already been created.  
        /// </summary>
        internal bool IsPassive { get; set; }

        /// <summary>
        /// The optional route key associated with the exchange definition.
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
        public void ApplyOverrides(ExchangeSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            IsPassive = settings.Passive ?? IsPassive;
            ContentType = settings.ContentType ?? ContentType;
            CancelRpcRequestAfterMs = settings.CancelRpcRequestAfterMs ?? CancelRpcRequestAfterMs;
            AlternateExchangeName = settings.AlternateExchange ?? AlternateExchangeName;
        }

        /// <summary>
        /// Route messages to this exchange if they cannot be routed.
        /// </summary>
        public string AlternateExchangeName { get; set; }

        /// <summary>
        /// The content type of the serialized message body.
        /// </summary>
        public string ContentType { get; set; } = SerializerTypes.Json;

        /// <summary>
        /// Number of milliseconds after which a RPC request will timeout
        /// if a response is not received from the message consumer.
        /// </summary>
        public int CancelRpcRequestAfterMs { get; set; } = SettingDefaults.RpcTimeOutAfterMs;

        /// <summary>
        /// Based on the type of exchange, used to identify a named action to the
        /// consumer processing the message.  
        /// </summary>
        public string ActionName { get; set; }

        public void LogProperties(IDictionary<string, object> log)
        {
            log["Exchange"] = IsDefaultExchange ? "Default-Exchange" : GetLogDetails();
            if (QueueMeta != null)
            {
                log["Queue"] = QueueMeta.GetLogDetails();
            }
        }

        public object GetLogDetails()
        {
            return new
            {
                BusName,
                ExchangeName,
                ExchangeType,
                MessageType = MessageType?.FullName ?? "n/a",
                IsAutoDelete,
                IsDurable,
                IsPassive,
                IsPersistent,
                IsRpcExchange,
                IsDefaultExchange,
                RouteKey,
                ContentType,
                CancelRpcRequestAfterMs = IsRpcExchange ? CancelRpcRequestAfterMs.ToString() : "n/a"
            };
        }
    }
}