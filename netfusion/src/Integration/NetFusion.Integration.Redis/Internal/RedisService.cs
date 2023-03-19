using NetFusion.Integration.Redis.Plugin;
using StackExchange.Redis;

namespace NetFusion.Integration.Redis.Internal
{
    /// <summary>
    /// Service that can be injected into another component to reference
    /// the entry-points into the Redis StackExchange client library.
    /// 
    /// NOTE:  Additional methods providing a higher level abstraction can
    /// be added to this service.  Also, extension methods can be added to
    /// the returned IDatabase and ISubscriber interfaces.
    /// </summary>
    public class RedisService : IRedisService
    {
        private readonly IConnectionModule _connModule;
        
        public RedisService(IConnectionModule connModule)
        {
            _connModule = connModule ?? throw new ArgumentNullException(nameof(connModule));
        }

        // Returns a named instance to a Redis database.
        public IDatabase GetDatabase(string name, int? database = null) => _connModule.GetDatabase(name, database);


        // Returns a named instance of the service used to subscribe to
        // Redis messages published to the database.
        public ISubscriber GetSubscriber(string name) => _connModule.GetSubscriber(name);
    }
}