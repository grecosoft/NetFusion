using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Plugin.Configs;
using NetFusion.Azure.ServiceBus.Settings;
using NetFusion.Base;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Settings;

namespace NetFusion.Azure.ServiceBus.Plugin.Modules
{
    /// <summary>
    /// Module responsible for managing Namespace related connections and metadata
    /// pertaining to the entities to be created within the namespaces.
    /// </summary>
    public class NamespaceModule : PluginModule,
        INamespaceModule
    {
        private BusSettings _busSettings;
        private readonly IDictionary<string, NamespaceConnection> _connections;
        
        public ServiceBusConfig BusConfig { get; private set; }

        // Injected Properties:
        public IEnumerable<INamespaceRegistry> Registries { get; private set; }

        public NamespaceModule()
        {
            _connections = new Dictionary<string, NamespaceConnection>();
        }

        public override void Initialize()
        {
            BusConfig = Context.Plugin.GetConfig<ServiceBusConfig>();

            AssertNamespaceRegistries();

            try
            {
                _busSettings = Context.Configuration.GetSettings(new BusSettings());
            }
            catch (SettingsValidationException ex)
            {
                NfExtensions.Logger.Log<NamespaceModule>(LogLevel.Error, ex.Message);
                throw;
            }
        }

        private void AssertNamespaceRegistries()
        {
            var duplicateNamespaces = Registries.WhereDuplicated(r => r.NamespaceName).ToArray();
            if (duplicateNamespaces.Any())
            {
                throw new InvalidOperationException(
                    $"More than one derived {typeof(NamespaceRegistryBase)} class defined for following " + 
                    $"namespaces {string.Join(", ", duplicateNamespaces)}");
            }
        }
        
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            foreach (NamespaceSettings busNs in _busSettings.Namespaces)
            {
                CreateNamespaceClient(busNs);
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

        private void CreateNamespaceClient(NamespaceSettings busNs)
        {
            if (_connections.ContainsKey(busNs.Name))
            {
                throw new InvalidOperationException(
                    $"The namespace {busNs.Name} is already configured.");
            }

            var connection = new NamespaceConnection(NfExtensions.Logger, busNs);
            connection.CreateClients();
            
            _connections[busNs.Name] = connection;
        }

        public NamespaceConnection GetConnection(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace must be specified.", nameof(namespaceName));
            
            if (_connections.TryGetValue(namespaceName, out NamespaceConnection client))
            {
                return client;
            }

            throw new InvalidOperationException(
                $"A client for the namespace {namespaceName} has not been configured.");
        }

        public SubscriptionSettings GetSubscriptionConfig(string namespaceName, string settingsKey)
        {
            var namespaceSettings = _busSettings.Namespaces.FirstOrDefault(n => n.Name == namespaceName);
            if (namespaceSettings == null)
            {
                throw new InvalidOperationException(
                    $"Could not read settings for namespace: {namespaceName}");
            }

            return namespaceSettings.Subscriptions.TryGetValue(settingsKey, out var config) ? config : null;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Registries"] = Registries.Select(r => new
            {
                r.NamespaceName,
                Type = r.GetType().FullName
            }).ToArray();
        }
    }
}