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
        [Required(AllowEmptyStrings = false, ErrorMessage = "BrokerName Required")]
        public string BrokerName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "HostName Required")]
        public string HostName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "VHostName Required")]
        public string VHostName { get; set; } = "/";

        [Required(AllowEmptyStrings = false, ErrorMessage = "UserName Required")]
        public string UserName { get; set; } = "guest";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password Required")]
        public string Password { get; set; } = "guest";

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
