using StackExchange.Redis;

namespace NetFusion.Integration.Redis.Internal
{
    /// <summary>
    /// Contains a reference to a Redis connection and the configuration
    /// options from which it was created.
    /// </summary>
    public class CachedConnection(
        ConfigurationOptions configuration,
        ConnectionMultiplexer connection)
    {
        public ConfigurationOptions Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));
        public ConnectionMultiplexer Connection { get; } = connection ?? throw new ArgumentNullException(nameof(connection));
    }
}