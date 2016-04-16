using RabbitMQ.Client;
using System;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Contains information for a queue that should be 
    /// created to consume response messages to RPC
    /// published messages.
    /// </summary>
    public class ReplyConsumer
    {
        public string ReplyQueueName { get; set; }
        public QueueingBasicConsumer Consumer { get; set; }
        public Type ReturnType { get; set; }
    }

}
