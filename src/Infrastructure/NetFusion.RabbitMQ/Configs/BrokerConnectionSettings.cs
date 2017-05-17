using NetFusion.Common;
using NetFusion.Utilities.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Contains properties for connecting to a given message broker.
    /// </summary>
    public class BrokerConnectionSettings : IValidatableType
    {
        /// <summary>
        /// The name key name of the broker used in code when declaring exchanges.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "BrokerName Required")]
        public string BrokerName { get; set; }

        /// <summary>
        /// The name of the host computer running RabbitMQ.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "HostName Required")]
        public string HostName { get; set; }

        /// <summary>
        /// The virtual host name.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "VHostName Required")]
        public string VHostName { get; set; } = "/";

        /// <summary>
        /// The user name to use when connecting to the broker.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "UserName Required")]
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// The password to use when connecting to the broker.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password Required")]
        public string Password { get; set; } = "guest";
        
        /// <summary>
        /// The configuration information for RPC queues defined by other applications
        /// to which this configured application can publish command messages.
        /// </summary>
        public IList<RpcConsumerSettings> RpcConsumers { get; set; }

        /// <summary>
        /// Additional properties that can be specified for a queue. 
        /// </summary>
        public IList<QueuePropertiesSettings> QueueProperties { get; set; }

        public BrokerConnectionSettings()
        {
            this.RpcConsumers = new List<RpcConsumerSettings>();
            this.QueueProperties = new List<QueuePropertiesSettings>();
        }

        /// <summary>
        /// Returns the configuration properties for a specified queue.
        /// </summary>
        /// <param name="queueName">The queue name to search.</param>
        /// <returns>The configured properties or a default instance.</returns>
        public QueuePropertiesSettings GetQueueProperties(string queueName)
        {
            Check.NotNull(queueName, nameof(queueName));

            QueuePropertiesSettings props = this.QueueProperties.FirstOrDefault(qp => qp.QueueName == queueName);
            return props ?? new QueuePropertiesSettings { QueueName = queueName };
        }

        public void Validate(IObjectValidator validator)
        {
            if (validator.IsValid)
            {
                RpcConsumers.Select(c => validator.AddChildValidator(c));
                QueueProperties.Select(p => validator.AddChildValidator(p));
            }
        }
    }
}
