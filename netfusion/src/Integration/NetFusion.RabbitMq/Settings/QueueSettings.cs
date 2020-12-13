using NetFusion.Base.Validation;

namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Queue settings specified within the application configuration.
    /// If specified they override all corresponding settings set in code.
    /// </summary>
    public class QueueSettings : IValidatableType
    {
        /// <summary>
        /// The name of the queue specified within the code.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The route keys to use when binding a queue to an exchange. 
        /// </summary>
        public string[] RouteKeys { get; set; }

        /// <summary>
        /// How long in milliseconds a message should remain on the queue before it is discarded.
        /// </summary>
        public int? PerQueueMessageTtl { get; set; }

        /// <summary>
        /// Indicates that any unacknowledged messages should be sent to the dead letter exchange
        /// and saved to any bound queues for future processing. 
        /// </summary>
        public bool? IsUnacknowledgedSaved { get; set; }

        /// <summary>
        /// Determines the maximum message priority that the queue should support.
        /// </summary>
        public byte? MaxPriority { get; set; }

        public ushort? PrefetchCount { get; set; }

        public int? Priority { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(! string.IsNullOrWhiteSpace(QueueName), $"{nameof(QueueName)} not specified.");
            
            validator.Verify(PerQueueMessageTtl == null || 0 <= PerQueueMessageTtl.Value, 
                $"{nameof(PerQueueMessageTtl)} must be greater or equal to zero.");
            
            validator.Verify(Priority == null || 0 < Priority, $"{nameof(Priority)} must be greater then zero.");

            validator.Verify(MaxPriority == null || 0 < MaxPriority, 
                $"{nameof(MaxPriority)} must be greater or equal to zero.");
        }
    }
}