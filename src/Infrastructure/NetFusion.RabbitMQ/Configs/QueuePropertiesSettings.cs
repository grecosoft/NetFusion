using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summaryıı
    /// Used to specify queue properties that are stored external to
    /// the queue definition defined in code.
    /// </summary>
    public class QueuePropertiesSettings 
    {
        public QueuePropertiesSettings()
        {
            this.RouteKeys = new List<string>();
        }

        /// <summary>
        /// The name of the queue associated with the configuration.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Queue Name Required")]
        public string QueueName { get; set; }

        /// <summary>
        /// The route keys that should be set on the queue.  If specified, any route
        /// keys defined in code will be overridden.
        /// </summary>
        public IList<string> RouteKeys { get; set; }

        /// <summary>
        /// The number of consumers that should subscribe to the queue
        /// and concurrently process messages.
        /// </summary>
        public int NumberConsumers { get; set; } = 1;
    }
}
