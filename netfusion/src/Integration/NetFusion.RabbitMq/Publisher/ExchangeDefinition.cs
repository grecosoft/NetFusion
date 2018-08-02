using IMessage = NetFusion.Messaging.Types.IMessage;
using System;
using NetFusion.Base;
using NetFusion.Base.Scripting;
using NetFusion.RabbitMQ.Settings;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Contains configuration information for an exchange to which messages can be delivered.
    /// </summary>
    public class ExchangeDefinition
    {
        /// <summary>
        /// The name of the configured message bus on which the exchange should be declared.
        /// This value is used to find the bus connection settings within the application's
        /// configuration file.
        /// </summary>
        public string BusName { get; }

        /// <summary>
        /// The type of the message associated with the exchange. 
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// The RabbitMQ type of the exchange to be created.  Null value represents 
        /// the default exchange. 
        /// </summary>
        public string ExchangeType { get; }

        /// <summary>
        /// The name of the exchange.  The name will be null if corresponding to the default exchange.
        /// </summary>
        public string ExchangeName { get; }

        /// <summary>
        /// The queue name defined on the default exchange.
        /// </summary>
        public string QueueName { get; }

        // Responsible for publishing the message based on the exchange type.
        internal IPublisherStrategy PublisherStrategy { get; private set; }

        private ExchangeDefinition(string busName, Type messageType)
        {
            BusName = busName ?? SettingDefaults.DefaultBusName;
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            PublisherStrategy = new DefaultPublisherStrategy();
        }

        public ExchangeDefinition(string busName, Type messageType, string exchangeType, string exchangeName):
            this(busName, messageType)
        {
            ExchangeType = exchangeType ?? throw new ArgumentNullException(nameof(exchangeType));
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        }

        public ExchangeDefinition(string busName, Type messageType, string queueName):
            this(busName, messageType)
        {
            QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        // Specifies the strategy to call when publishing a message to the exchange.
        internal void SetPublisherStrategy(IPublisherStrategy strategy)
        {
            PublisherStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }
        
        /// <summary>
        /// Indicates a definition of a work queue on the default exchange.
        /// </summary>
        /// <returns></returns>
        public bool IsDefaultExchangeQueue => 
            ExchangeType == null && ExchangeName == null && QueueName != null;
                   
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
        public int CancelRpcRequestAfterMs = SettingDefaults.RpcTimeOutAfterMs;

        /// <summary>
        /// Overrides and default values specified within code for a given
        /// exchange type with the corresponding values if present from the
        /// application's configuration.
        /// </summary>
        /// <param name="configuredSettings">The settings for the exchange
        /// specified within the application's configuration file.</param>
        public void ApplyOverrides(ExchangeSettings configuredSettings)
        {
            if (configuredSettings == null)
                throw new ArgumentNullException(nameof(configuredSettings));

            IsPassive = configuredSettings.Passive;
            AlternateExchangeName = configuredSettings.AlternateExchange;
            CancelRpcRequestAfterMs = configuredSettings.CancelRpcRequestAfterMs;
            ContentType = configuredSettings.ContentType ?? ContentType;
        }

        // Delegate that is called to determine if the message being published
        // should be delivered to the exchange.
        internal Func<IMessage, bool> DelegatePredicate { get; private set; } = m => true;

        // Optional metadata for a runtime expression that should be dynamically applied
        // to the message to yield a predicate value indicating if the message applies.
        internal ScriptPredicate ScriptPredicate { get; private set; }


        /// <summary>
        /// Optionally defines the script that should be invoked on the message
        /// to determine if the message should be published to the exchange.
        /// </summary>
        public void Where(Func<IMessage, bool> predicate)
        {
            DelegatePredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Sets the name of an external script to be called on the message being published to
        /// determine if it should be delivered to the exchange.
        /// </summary>
        /// <param name="scriptName">The name of the externally defined defined script to be
        /// executed on the message.</param>
        /// <param name="attributeName">The Boolean attribute value calculated by the script 
        /// and set on the message.  If True, the message will be published to the exchange.</param>
        public void Where(string scriptName, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(scriptName)) throw new ArgumentException("message", nameof(scriptName));
            if (string.IsNullOrWhiteSpace(attributeName)) throw new ArgumentException("message", nameof(attributeName));

            ScriptPredicate = new ScriptPredicate {
                ScriptName = scriptName,
                AttributeName = attributeName
            };
        }
    }
}