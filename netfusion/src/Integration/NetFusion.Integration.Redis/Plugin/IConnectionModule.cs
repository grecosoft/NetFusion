using NetFusion.Core.Bootstrap.Plugins;
using StackExchange.Redis;

namespace NetFusion.Integration.Redis.Plugin
{
    /// <summary>
    /// Service module used to manage Redis connections and obtaining the main object
    /// instances used to issue commands and create subscribers.
    /// </summary>
    public interface IConnectionModule : IPluginModuleService
    {
        /// <summary>
        /// Returns a named instance to a Redis database.
        /// </summary>
        /// <param name="name">The configured name of the database.</param>
        /// <param name="database">Id value of the database.  Used to partition a Redis instance into separate databases.</param>
        /// <returns>Reference to the database or an exception if specified database is not configured.</returns>
        IDatabase GetDatabase(string name, int? database = null);

        /// <summary>
        /// Returns a named instance of the service used to subscribe to Redis messages published to the database.
        /// </summary>
        /// <param name="name">The configured name of the database.</param>
        /// <returns>Reference to the subscription service or an exception if specified database
        /// is not configured.</returns>
        ISubscriber GetSubscriber(string name);
    }
}
