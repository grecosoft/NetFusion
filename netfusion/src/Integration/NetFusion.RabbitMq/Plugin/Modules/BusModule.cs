using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Settings;
using NetFusion.Settings;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("IntegrationTests")]
namespace NetFusion.RabbitMQ.Plugin.Modules
{
    /// <summary>
    /// The main plugin module providing access to the configured IBus instances used to
    /// communicate with RabbitMQ servers.  Each bus is identified by a name specified 
    /// within the application's configuration.
    /// </summary>
    public class BusModule : PluginModule,
        IBusModule
    {
        private BusSettings _busSettings;
        
        // The bus instances keyed by name created from BusSettings.
        private readonly Dictionary<string, IBus> _buses;
        private ISerializationManager _serializationMgr;

        private bool _disposed;

        public BusModule()
        {
            _buses = new Dictionary<string, IBus>();
        }
        
        // Unique value identifying the host plugin.  The value can be used to tag exchanges
        // and queues so the associated host can be identified.  This will make a given queue
        // name unique to a given application host allowing for messages to be delivered round
        // robin among a set of running application instances.
        public string HostAppId => Context.AppHost.PluginId;

        // Creates IBus instances for each configured bus.
        public override void Initialize()
        {
            _busSettings = Context.Configuration.GetSettings(Context.Logger, new BusSettings());

            foreach (BusConnection conn in _busSettings.Connections)
            {
                CreateBus(conn);
            }
        }

        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            _serializationMgr = services.GetService<ISerializationManager>();
            if (_serializationMgr == null)
            {
                Context.Logger.LogError("A serialization manager has not been registered.");
            }

            return base.OnStartModuleAsync(services);
        }

        private void CreateBus(BusConnection conn)
        {
            if (_buses.ContainsKey(conn.BusName))
            {
                throw new ContainerException(
                    $"A bus has already been created for the bus named: {conn.BusName}." + 
                    "Check configuration for duplicates.");
            }

            // Create an EasyNetQ connection configuration from settings:
            ConnectionConfiguration connConfig = new ConnectionConfiguration {
                UserName = conn.UserName,
                Password = conn.Password,
                VirtualHost = conn.VHostName,
                RequestedHeartbeat = conn.Heartbeat,
                Name = conn.BusName,
                Product = Context.AppHost.Name,
                Timeout =  conn.Timeout,
                PublisherConfirms = conn.PublisherConfirms,
                PersistentMessages = conn.PersistentMessages,
                UseBackgroundThreads = conn.UseBackgroundThreads,
                PrefetchCount = conn.PrefetchCount
            };

            SetAdditionalClientProperties(connConfig.ClientProperties);

            // Set associated hosts:
            connConfig.Hosts = conn.Hosts.Select(h => new HostConfiguration {
                Host = h.HostName,
                Port = h.Port

            }).ToArray();

            // Allow EasyNetQ to validate the connection configuration:
            connConfig.Validate();
    
            _buses[conn.BusName] = BusFactory(connConfig);
        }

        /// <summary>
        /// Factory method used to return an IBus implementation.  Default to EasyNetQ but can also
        /// be used to provided a mock during unit testing.
        /// </summary>
        protected Func<ConnectionConfiguration, IBus> BusFactory = c => RabbitHutch.CreateBus(c, rs => {});

        // Additional client properties associated with created connections.
        private void SetAdditionalClientProperties(IDictionary<string, object> clientProps)
        {
            IPlugin rabbitPlugin = Context.Plugin;
            IPlugin appHostPlugin = Context.AppHost;
        
            clientProps["Client Assembly"] = rabbitPlugin.AssemblyName;
            clientProps["Client Version"] = rabbitPlugin.AssemblyVersion;
            clientProps["AppHost Name"] = appHostPlugin.Name;
            clientProps["AppHost Assembly"] = appHostPlugin.AssemblyName;
            clientProps["AppHost Version"] = appHostPlugin.AssemblyVersion;
            clientProps["AppHost Description"] = appHostPlugin.Description;
            clientProps["Machine Name"] = Environment.MachineName;          
        }

        public IBus GetBus(string named)
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Bus name not specified.", nameof(named));

            if (! _buses.TryGetValue(named, out IBus bus))
            {
                throw new InvalidOperationException(
                    $"The bus named: {named} has not been configured.  Check application configuration.");
            }
            return bus;
        }

        public void ApplyExchangeSettings(ExchangeMeta meta)
        {
            ApplyExchangeSettingsInternal(meta);
        }

        public void ApplyQueueSettings(QueueMeta meta)
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            var queueSettings = GetQueueSettings(meta.Exchange.BusName, meta.QueueName);
            if (queueSettings != null)
            {
                meta.ApplyOverrides(queueSettings);
            }
            
            ApplyExchangeSettingsInternal(meta.Exchange, applyQueueSettings: false);
        }
        
        private void ApplyExchangeSettingsInternal(ExchangeMeta meta, bool applyQueueSettings = true)
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta));
            
            if (! meta.IsDefaultExchange)
            {
                var exchangeSettings = GetExchangeSettings(meta.BusName, meta.ExchangeName);
                if (exchangeSettings != null)
                {
                    meta.ApplyOverrides(exchangeSettings);
                }
            }
            
            if (applyQueueSettings && meta.QueueMeta != null)
            {
                ApplyQueueSettings(meta.QueueMeta);
            }
        }

        private BusConnection GetBusConnection(string busName)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));

            BusConnection connection = _busSettings.Connections.FirstOrDefault(c => c.BusName == busName);
            if (connection == null)
            {
                throw new InvalidOperationException(
                    $"A bus configuration with the name: {busName} has not been configured.");
            }

            return connection;
        }

        private ExchangeSettings GetExchangeSettings(string busName, string exchangeName)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name not specified.", nameof(exchangeName));

            var busConn = GetBusConnection(busName);
            return busConn.ExchangeSettings.FirstOrDefault(s => s.ExchangeName == exchangeName);
        }
        
        private QueueSettings GetQueueSettings(string busName, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(queueName));

            var busConn = GetBusConnection(busName);
            return busConn.QueueSettings.FirstOrDefault(s => s.QueueName == queueName);
        }

        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(IBus bus in _buses.Values)
            {
                bus.Dispose();
            }

            _disposed = true;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Bus-Connections"] = _busSettings.Connections.Select( c => new {
                c.BusName,
                c.UserName,
                c.VHostName,
                c.Timeout,
                c.PublisherConfirms,
                c.PersistentMessages,
                c.UseBackgroundThreads,
                c.PrefetchCount,
                Hosts = c.Hosts.Select(h => new { h.HostName, h.Port })
            }).ToArray();

            moduleLog["Serialization-Providers"] = new {
                SerializationManager = _serializationMgr.GetType().FullName,
                Serializers = _serializationMgr.Serializers.Select(s => new {
                    s.ContentType,
                    SerializerType = s.GetType().FullName
                }).ToArray()
            };
        }
    }
}