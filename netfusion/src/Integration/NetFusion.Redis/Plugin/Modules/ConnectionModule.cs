using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Redis.Internal;
using NetFusion.Redis.Settings;
using NetFusion.Settings;
using StackExchange.Redis;

namespace NetFusion.Redis.Plugin.Modules
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
        private readonly Dictionary<string, CachedConnection> _connections;
        private RedisSettings _redisSettings;

        public ConnectionModule()
        {
            _connections = new Dictionary<string, CachedConnection>();            
        }
        
        //------------------------------------------------------
        //--Plugin Initialization
        //------------------------------------------------------

        public override void Configure()
        {
            try
            {
                _redisSettings = Context.Configuration.GetSettings(new RedisSettings());
            }
            catch (SettingsValidationException ex)
            {
                Context.BootstrapLogger.Add(LogLevel.Error, ex.Message);
            }
           
            AssertSettings(_redisSettings);
        }
        
        private static void AssertSettings(RedisSettings settings)
        {
            var duplicateConnNames = settings.Connections
                .WhereDuplicated(s => s.Name)
                .ToArray();

            if (duplicateConnNames.Any())
            {
                throw new ContainerException("Configured database names must be unique.", 
                    "duplicate-connection-names", 
                    duplicateConnNames);
            }
        }
        
        //------------------------------------------------------
        //--Plugin Execution
        //------------------------------------------------------

        protected override async Task OnStartModuleAsync(IServiceProvider services)
        {
            foreach (DbConnection connSetting in _redisSettings.Connections)
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
                var logger = new RedisLogTextWriter(Context.Logger);
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

        //------------------------------------------------------
        //--Plugin Services
        //------------------------------------------------------

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
            
            if (! _connections.TryGetValue(connectionName, out CachedConnection cachedConn))
            {
                throw new InvalidOperationException(
                    $"A configured Redis database connection name: {connectionName} is not configured.");
            }
            
            if (! cachedConn.Connection.IsConnected)
            {
                Context.Logger.LogError(RedisLogEvents.ConnException, 
                    "Requested database connection {dbConfigName} is currently in a disconnected state.",
                    connectionName);
            }

            return cachedConn;
        }

        // https://github.com/grecosoft/NetFusion/wiki/core.logging.composite#module-logging
        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["redis:connections"] = _redisSettings.Connections.Select(c => new
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
