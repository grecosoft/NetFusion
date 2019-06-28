using StackExchange.Redis;

namespace NetFusion.Redis
{
    /// <summary>
    /// Service allows access to the configured Redis databases and subscribers.
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// Returns a named instance to a Redis database.
        /// </summary>
        /// <param name="name">The configured name of the database.</param>
        /// <param name="database">Id value of the database.  Used to partition
        /// a Redis instance into separate databases.</param>
        /// <returns>Reference to the database or an exception if specified
        /// database is not configured.</returns>
        IDatabase GetDatabase(string name, int? database = null);

        /// <summary>
        /// Returns a named instance of the service used to subscribe to
        /// Redis messages published to the database.
        /// </summary>
        /// <param name="name">The configured name of the database.</param>
        /// <returns>Reference to the subscription service or an exception
        /// if specified database is not configured.</returns>
        ISubscriber GetSubscriber(string name);
    }
}