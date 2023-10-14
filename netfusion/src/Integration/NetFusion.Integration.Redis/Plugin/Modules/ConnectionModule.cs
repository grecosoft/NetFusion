using Microsoft.Extensions.Logging;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Core.Settings;
using NetFusion.Integration.Redis.Internal;
using NetFusion.Integration.Redis.Plugin.Settings;
using StackExchange.Redis;

namespace NetFusion.Integration.Redis.Plugin.Modules
{
    /// <summary>
    /// Module responsible for creating and managing the application host's
    /// Redis connections.  Each connection has an associated name.
    /// </summary>
    public class ConnectionModule : PluginModule,
        IConnectionModule
    {
        // Maps connection name specified within the host's application settings
        // to the corresponding created connection. 
        private readonly Dictionary<string, CachedConnection> _connections = new();
        private RedisSettings? _redisSettings;

        private RedisSettings RedisSettings => _redisSettings ?? 
            throw new NullReferenceException("Settings have not been initialize.");
        
        private ILogger<ConnectionModule> Logger => Context.LoggerFactory.CreateLogger<ConnectionModule>();

        public override void Configure()
        {
            try
            {
                _redisSettings = Context.Configuration.GetSettings(new RedisSettings());
                _redisSettings.SetNamedConfigurations();
            }
            catch (SettingsValidationException ex)
            {
                Logger.LogError(ex, "Validation Exception");
                throw;
            }
        }
        
        protected override async Task OnStartModuleAsync(IServiceProvider services)
        {
            foreach (var connSetting in RedisSettings.Connections.Values)
            {
                var connOptions = new ConfigurationOptions
                {
                    // After connection failure, retry connecting.
                    AbortOnConnectFail = false, 
                    KeepAlive = connSetting.KeepAlive,
                    ConnectRetry = connSetting.ConnectRetry,
                    ConnectTimeout = connSetting.ConnectTimeout,
                    Password = connSetting.Password,
                    DefaultDatabase = connSetting.DefaultDatabaseId,
                    ClientName = Context.AppHost.Name
                };

                foreach (var endPointSetting in connSetting.EndPoints)
                {
                    connOptions.EndPoints.Add(endPointSetting.Host, endPointSetting.Port);
                }

                // Create and cache the connection.
                var logger = new RedisLogTextWriter(Logger);
                var connection = await ConnectionMultiplexer.ConnectAsync(connOptions, logger);
                
                _connections[connSetting.Name] = new CachedConnection(connOptions, connection);
            }
        }

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach(CachedConnection cachedConn in _connections.Values)
            {
                await cachedConn.Connection.CloseAsync();
            }
        }

        public IDatabase GetDatabase(string connectionName, int? database = null)
        {
            CachedConnection cachedConn = GetConnection(connectionName);
            int? databaseId = database ?? cachedConn.Configuration.DefaultDatabase;

            return databaseId == null
                ? cachedConn.Connection.GetDatabase()
                : cachedConn.Connection.GetDatabase(databaseId.Value);
        }

        public ISubscriber GetSubscriber(string connectionName)
        {
            CachedConnection cachedConn = GetConnection(connectionName);
            return cachedConn.Connection.GetSubscriber();
        }
        
        private CachedConnection GetConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                throw new ArgumentException("Name of database not specified.", nameof(connectionName));
            
            if (! _connections.TryGetValue(connectionName, out CachedConnection? cachedConn))
            {
                throw new RedisPluginException(
                    $"A configured Redis database connection name: {connectionName} is not configured.");
            }
            
            if (! cachedConn.Connection.IsConnected)
            {
                Logger.LogError("Requested database connection {dbConfigName} is currently in a disconnected state.",
                    connectionName);
            }

            return cachedConn;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["RedisConnections"] = RedisSettings.Connections.Values.Select(c => new
            {
                DatabaseName = c.Name,
                EndPoints = c.EndPoints.Select(ep => new
                {
                    ep.Host,
                    ep.Port
                })
            });
        }
    }
}
