using NetFusion.Common.Base;
using NetFusion.Core.Settings;
using NetFusion.Integration.ServiceBus.Internal;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Plugin.Configs;
using NetFusion.Integration.ServiceBus.Plugin.Settings;

namespace NetFusion.Integration.ServiceBus.Plugin.Modules;

    /// <summary>
    /// Module responsible for managing Namespace connections and configurations.
    /// </summary>
    internal class NamespaceModule : PluginModule,
        INamespaceModule
    {
        private ServiceBusConfig? _busPluginConfig;
        private BusSettings? _busSettings;
        private readonly IDictionary<string, NamespaceConnection> _connections = new Dictionary<string, NamespaceConnection>();
        
        public ServiceBusConfig BusPluginConfiguration => _busPluginConfig ??
            throw new NullReferenceException("Bus Configuration not Initialized");

        private BusSettings BusSettings => _busSettings ?? 
            throw new NullReferenceException("Bus Settings not Initialized");
        
        private ILogger<NamespaceModule> Logger => Context.LoggerFactory.CreateLogger<NamespaceModule>();

        public override void Initialize()
        {
            _busPluginConfig = Context.Plugin.GetConfig<ServiceBusConfig>();
            
            try
            {
                _busSettings = Context.Configuration.GetSettings(new BusSettings());
                _busSettings.InitConfiguration();
            }
            catch (SettingsValidationException ex)
            {
                Logger.LogError(ex, "Validation Exception");
                throw;
            }
        }

        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            foreach (NamespaceSettings busNs in BusSettings.Namespaces.Values)
            {
                CreateNamespaceClient(services, busNs);
            }

            return base.OnStartModuleAsync(services);
        }

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach (NamespaceConnection conn in _connections.Values)
            {
                await conn.CloseClientsAsync();
            }
        }

        private void CreateNamespaceClient(IServiceProvider services, NamespaceSettings busNs)
        {
            if (_connections.ContainsKey(busNs.Name))
            {   
                throw new ServiceBusPluginException(
                    $"The namespace {busNs.Name} is already configured.");
            }

            busNs.HostPluginId = Context.AppHost.PluginId;
            
            var logger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger<NamespaceConnection>();
            
            _connections[busNs.Name] = new NamespaceConnection(logger, busNs);
        }

        public NamespaceConnection GetConnection(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace must be specified.", nameof(namespaceName));
            
            if (_connections.TryGetValue(namespaceName, out NamespaceConnection? client))
            {
                return client;
            }

            throw new ServiceBusPluginException(
                $"A client for the namespace {namespaceName} has not been configured.", "NAMESPACE_CONN_NOTFOUND");
        }
    }