using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.RabbitMQ.Settings;
using NetFusion.Settings;

[assembly: InternalsVisibleTo("IntegrationTests")]
namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// The main plugin module providing access to the configured IBus instances used to
    /// communicate with RabbitMQ servers.  Each bus is identified by a name specified 
    /// within the configuration. 
    /// </summary>
    public class BusModule : PluginModule,
        IBusModule
    {
        private BusSettings _busSettings;
        
        // The bus instances keyed by name created from BusSettings.
        private readonly Dictionary<string, IBus> _busses;
        private ISerializationManager _serializationMgr;

        private bool _disposed;

        public BusModule()
        {
            _busses = new Dictionary<string, IBus>();
        }

        // Creates IBus instances for each configured bus.
        public override void Initialize()
        {
            _busSettings = Context.Configuration.GetSettings<BusSettings>(Context.Logger);

            foreach (BusConnection conn in _busSettings.Connections)
            {
                CreateBus(conn);
            }
        }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ISerializationManager, SerializationManager>();
        }
        
        public override void StartModule(IServiceProvider services)
        {
            _serializationMgr = services.GetService<ISerializationManager>();
            if (_serializationMgr == null)
            {
                Context.Logger.LogError("A serialization manager has not been registered.");
            }
        }

        private void CreateBus(BusConnection conn)
        {
            if (_busses.ContainsKey(conn.BusName))
            {
                throw new ContainerException(
                    $"A bus has already been created for the bus named: {conn.BusName}." + 
                    "Check configuration for duplicates.");
            }

            ConnectionConfiguration connConfig = new ConnectionConfiguration {
                UserName = conn.UserName,
                Password = conn.Password,
                VirtualHost = conn.VHostName,
                RequestedHeartbeat = conn.Heartbeat,
                Name = conn.BusName,
                Product = Context.AppHost.Manifest.Name,
                Timeout =  conn.Timeout,
                PublisherConfirms = conn.PublisherConfirms,
                PersistentMessages = conn.PersistentMessages,
                UseBackgroundThreads = conn.UseBackgroundThreads,
                PrefetchCount = conn.PrefetchCount
            };

            SetAdditionalClientProperties(connConfig.ClientProperties);

            connConfig.Hosts = conn.Hosts.Select(h => new HostConfiguration {
                Host = h.HostName,
                Port = h.Port

            }).ToArray();

            connConfig.Validate();
    
            _busses[conn.BusName] = BusFactory(connConfig);
        }

        public Func<ConnectionConfiguration, IBus> BusFactory = c => RabbitHutch.CreateBus(c, rs => {});

        // Additional client properties assocated with created connections.
        private void SetAdditionalClientProperties(IDictionary<string, object> clientProps)
        {
            IPluginManifest rabbitPlugin = Context.Plugin.Manifest;
            IPluginManifest appHostPlugin = Context.AppHost.Manifest;
        
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

            if (! _busses.TryGetValue(named, out IBus bus))
            {
                throw new InvalidOperationException($"The bus named: {named} has not been configured.");
            }
            return bus;
        }

        public ExchangeSettings GetExchangeSettings(string busName, string exchangeName)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name not specified.", nameof(exchangeName));

            var busConn = GetBusConnection(busName);
            return busConn.ExchangeSettings.FirstOrDefault(s => s.ExchangeName == exchangeName);
        }

        public QueueSettings GetQueueSettings(string busName, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(queueName));

            var busConn = GetBusConnection(busName);
            return busConn.QueueSettings.FirstOrDefault(s => s.QueueName == queueName);
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

        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(IBus bus in _busses.Values)
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