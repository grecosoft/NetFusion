using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Used to specify queue properties that are stored external to
    /// the queue definition defined in code.
    /// </summary>
    public class QueueProperties
    {
        public QueueProperties()
        {
            this.RouteKeys = new string[] { };
        }

        public string QueueName { get; set; }
        public IEnumerable<string> RouteKeys { get; set; }
    }
}
