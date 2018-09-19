using System;

namespace NetFusion.Redis.Subscriber
{
    /// <summary>
    /// Attribute specified on an event handler method used to
    /// subscribe to a Redis channel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ChannelSubscriptionAttribute : Attribute
    {
        /// <summary>
        /// Database name as specified with the application's configuration.
        /// </summary>
        public string DatabaseName { get; }
        
        /// <summary>
        /// Specific channel name or channel name based on a pattern.
        /// </summary>
        public string Channel { get; }
        
        /// <summary>
        /// Associates the method with a Redis channel.
        /// </summary>
        /// <param name="databaseName">Database name as specified with the application's configuration.</param>
        /// <param name="channel">Specific channel name or channel name based on a pattern.</param>
        public ChannelSubscriptionAttribute(string databaseName, string channel)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name is not specified.", nameof(databaseName));
            
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel name is not specified.", nameof(channel));

            DatabaseName = databaseName;
            Channel = channel;
        }
    }
}