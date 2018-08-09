using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetFusion.Base.Validation;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.RabbitMQ.Settings
{
    public class BusConnection : IValidatableType
    {
        /// <summary>
        /// The name of the bus used in code when declaring exchanges and queues.
        /// If the name of the bus is not specified within the code, the default 
        /// name of: message-bus is assumed.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "BrokerName Required")]
        public string BusName { get; set; }

        /// <summary>
        /// One or more hosts associated with the connection.
        /// </summary>
        public IList<BusHost> Hosts { get; set;}

        public BusConnection()
        {
            Hosts = new List<BusHost>();
        }

        /// <summary>
        /// The connections heartbeat.  Defaults to 10.
        /// </summary>
        public ushort Heartbeat { get; set; } = 10;

        /// <summary>
        /// The virtual host name.  Defaults to /.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "VHostName Required")]
        public string VHostName { get; set; } = "/";

        /// <summary>
        /// The user name to use when connecting to the broker.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "UserName Required")]
        public string UserName { get; set; } 

        /// <summary>
        /// The password to use when connecting to the broker.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password Required")]
        public string Password { get; set; }

        /// <summary>
        /// The connection timeout.
        /// </summary>
        public ushort Timeout { get; set; } = 10;

        /// <summary>
        /// Extension to AMQP that provides a callback when your message has been successfully received by the broker.
        /// </summary>
        public bool PublisherConfirms { get; set; } = false;

        /// <summary>
        /// When set to true, messages will be persisted to disk by RabbitMQ and survive a server restart.
        /// Performance gains can be expected when set to false.
        /// </summary>
        public bool PersistentMessages { get; set; } = true;

        /// <summary>
        /// Indicates if multiple background threads should be used.
        /// </summary>
        public bool UseBackgroundThreads { get; set; } = false;
        
        /// <summary>
        /// The number of seconds to wait before trying to reconnect.
        /// </summary>
        public TimeSpan ConnectIntervalAttempt { get; set; } = TimeSpan.FromSeconds(5.0);

        /// <summary>
        /// This is the number of messages that will be delivered by RabbitMQ before an ack is sent by EasyNetQ.
        /// Set to 0 for infinite prefetch (not recommended). Set to 1 for fair work balancing among a farm of consumers.
        /// </summary>
        public ushort PrefetchCount { get; set; } = 50;

        /// <summary>
        /// Exchange settings stored external from the code.
        /// </summary>
        /// <returns>Collection of exchange settings.</returns>
        public ExchangeSettings[] ExchangeSettings { get; set; } = Array.Empty<ExchangeSettings>();

        /// <summary>
        /// Queue settings stored external from the code.
        /// </summary>
        /// <returns>Collection of queue settings.</returns>
        public QueueSettings[] QueueSettings { get; set; } = Array.Empty<QueueSettings>();


        public void Validate(IObjectValidator validator)
        {
            validator.AddChild(ExchangeSettings);
            validator.AddChild(QueueSettings);

            Hosts.ForEach(h => validator.AddChild(h));
        }
    }
}