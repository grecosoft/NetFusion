using NetFusion.Common.Base.Validation;

namespace NetFusion.Integration.RabbitMQ.Plugin.Settings
{
    /// <summary>
    /// Queue settings specified within the application configuration.
    /// If specified they override all corresponding settings set in code.
    /// </summary>
    public class QueueSettings : IValidatableType
    {
        public string QueueName { get; set; } = null!;
        
        public string[]? RouteKeys { get; set; }
        public bool? IsDurable { get; set; } 
        public bool? IsExclusive { get; set; } 
        public bool? IsAutoDelete { get; set; }
        public int? MaxPriority { get; set; }
        public int? PerQueueMessageTtl { get; set; }
        public string? DeadLetterExchangeName { get; set; }
        public ushort? PrefetchCount { get; set; }
        public int? Priority { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(PerQueueMessageTtl is null or >= 0, 
                $"{nameof(PerQueueMessageTtl)} must be greater or equal to zero.");
            
            validator.Verify(Priority is null or > 0, $"{nameof(Priority)} must be greater then zero.");
            validator.Verify(MaxPriority is null or > 0, $"{nameof(MaxPriority)} must be greater or equal to zero.");
        }
    }
}