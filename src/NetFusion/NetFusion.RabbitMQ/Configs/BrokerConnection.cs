using NetFusion.Common;
using NetFusion.Common.Validation;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Contains properties for connecting to a given message broker.
    /// </summary>
    public class BrokerConnection : IObjectValidation
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
        public IEnumerable<RpcConsumerSettings> RpcConsumers { get; set; } 

        public BrokerConnection()
        {
            this.RpcConsumers = new List<RpcConsumerSettings>();
            this.QueueProperties = new List<QueueProperties>();
        }

        /// <summary>
        /// Additional properties that can be specified for a queue. 
        /// </summary>
        public IEnumerable<QueueProperties> QueueProperties { get; set; }

        // The established connection.
        internal IConnection Connection { get; set; }

        /// <summary>
        /// Returns the configuration properties for a specified queue.
        /// </summary>
        /// <param name="queueName">The queue name to search.</param>
        /// <returns>The configured properties or a default instance.</returns>
        public QueueProperties GetQueueProperties(string queueName)
        {
            Check.NotNull(queueName, nameof(queueName));

            QueueProperties props = this.QueueProperties.FirstOrDefault(qp => qp.QueueName == queueName);
            return props ?? new QueueProperties { QueueName = queueName };
        }

        /// <summary>
        /// Validates the configuration object after it's state is loaded.
        /// </summary>
        /// <returns>The result of the validation.</returns>
        public ObjectValidator ValidateObject()
        {
            var valResult = new ObjectValidator(this);

            if (valResult.IsValid)
            {
                IEnumerable<ObjectValidator> rpcValResults = this.RpcConsumers.Select(c => c.ValidateObject());
                valResult.AddChildValidations(rpcValResults);

                IEnumerable<ObjectValidator> queuePropResults = this.QueueProperties.Select(c => c.ValidateObject());
                valResult.AddChildValidations(queuePropResults);
            }
        
            return valResult;
        }
    }
}
