using System;
using Azure.Messaging.ServiceBus;

namespace NetFusion.Azure.ServiceBus.Settings
{
    /// <summary>
    /// Allows specifying retry settings used when the connection
    /// to the namespace is created.
    /// </summary>
    public class RetrySettings
    {
        /// <summary>
        /// The approach to use for calculating retry delays.
        /// </summary>
        public ServiceBusRetryMode? Mode { get; set; }
        
        /// <summary>
        /// The maximum number of retry attempts before considering the associated operation
        /// to have failed.
        /// </summary>
        public int? MaxRetries { get; set; }
        
        /// <summary>
        /// The delay between retry attempts for a fixed approach or the delay
        /// on which to base calculations for a backoff-based approach.
        /// </summary>
        public TimeSpan? Delay { get; set; }
        
        /// <summary>
        /// The maximum permissible delay between retry attempts.
        /// </summary>
        public TimeSpan? MaxDelay { get; set; }
        
        /// <summary>
        /// The maximum duration to wait for completion of a single attempt, whether the initial
        /// attempt or a retry.
        /// </summary>
        public TimeSpan? TryTimeout { get; set; }
    }
}