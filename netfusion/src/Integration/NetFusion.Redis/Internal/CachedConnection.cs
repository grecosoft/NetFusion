using System;
using StackExchange.Redis;

namespace NetFusion.Redis.Internal
{
    /// <summary>
    /// Contains a reference to a Redis connection and the configuration
    /// options from which it was created.
    /// </summary>
    public class CachedConnection
    {
        public ConfigurationOptions Configration { get; }
        public ConnectionMultiplexer Connection { get; }

        public CachedConnection(
            ConfigurationOptions configuration, 
            ConnectionMultiplexer connection)
        {
            Configration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
    }
}