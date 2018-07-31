using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Contains properties describing the exchange to which a queue is being bound and 
    /// scribed to by the consumer.
    /// </summary>
    public class QueueExchangeDefinition
    {
        /// <summary>
        /// The name of the configured message bus on which the exchange should be declared.
        /// </summary>
        public string BusName { get; set; } = SettingDefaults.DefaultBusName;

        /// <summary>
        /// The name of the exchange.  The name will be null if corresponding to the default exchange.
        /// This is the case where the RouteKey is the queue name.  This is the case for the MessageQueue
        /// exchange type.  
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// The RabbitMQ type of the exchange to be created. 
        /// </summary>
        public string ExchangeType { get; set; }

        /// <summary>
        /// Indicates a definition of a work queue on the default exchange.
        /// </summary>
        /// <returns></returns>
        public bool IsDefaultExchange => 
            ExchangeType == null && ExchangeName == null;
       
        /// <summary>
        ///  Do not create an exchange. If the named exchange doesn't exist, throw an exception.
        /// </summary>
        public bool IsPassive { get; set; }

         /// <summary>
        /// Survive server restarts. If this parameter is false, the exchange will be removed when the
        /// server restarts.
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// Delete this exchange when the last queue is unbound.
        /// </summary>
        public bool IsAutoDelete { get; set; }

        /// <summary>
        /// Route messages to this exchange if they cannot be routed.
        /// </summary>
        public string AlternateExchange { get; set; }

        /// <summary>
        /// Overrides any values from settings specified within the application's configuration.
        /// </summary>
        /// <param name="configuredSettings"></param>
        public void ApplyOverrides(ExchangeSettings configuredSettings)
        {
            if (configuredSettings == null) throw new System.ArgumentNullException(nameof(configuredSettings));

            IsPassive = configuredSettings.Passive;
            AlternateExchange = configuredSettings.AlternateExchange;
        } 
    }
}