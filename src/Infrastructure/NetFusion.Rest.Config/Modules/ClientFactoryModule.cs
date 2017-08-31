using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.Config.Core;

namespace NetFusion.Rest.Config.Modules
{
    /// <summary>
    /// Plugin-Module responsible for reading request client aplication settings
    /// and configuring the RequestClientFactory instance.  A given configuation
    /// can specify an entry-point URL used to describe initial URL called to 
    /// return resources managed by the service.  After this point, the client
    /// navigates using links relations assocted with the returned resource.
    /// </summary>
    public class ClientFactoryModule : PluginModule,
        IClientFactoryModule
    {
        private bool _disposed;

        private Dictionary<string, string> _addressNameMappings;
        private Dictionary<string, Lazy<HalEntryPointResource>> _entryPointMappings;

        public ClientFactoryModule()
        {
            _entryPointMappings = new Dictionary<string, Lazy<HalEntryPointResource>>();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<ClientFactoryComponent>()
                .As<IRequestClientFactory>()
                .SingleInstance();
        }

        public override void StartModule(IContainer container, ILifetimeScope scope)
        {
            var factorySettings = scope.Resolve<ClientFactorySettings>();
            if (factorySettings == null || factorySettings.Clients == null)
            {
                return;
            }

            CreateAddressNameMapping(factorySettings);
            RegisterClientsWithFactory(factorySettings);
            LazyLoadClientEntryPoints(factorySettings);
        }        

        // The RequestFactory can be used outside of the NetFusion.Rest.Confg plug-in.
        // The RequestFactory tracks RequestClients by their associated base address.
        // When using NetFusion.Rest.Config, a simple name can be assocated with the
        // base address.
        private void CreateAddressNameMapping(ClientFactorySettings factorySettings)
        {
            _addressNameMappings = factorySettings.Clients
                .ToDictionary(c => c.ClientName, c => c.BaseAddress);
        }

        private void RegisterClientsWithFactory(ClientFactorySettings factorySettings)
        {
            RequestClientFactory.Instance.RegisterMediaTypeSerializer<JsonMediaTypeSerializer>();
            RequestClientFactory.Instance.RegisterMediaTypeSerializer<JsonMediaTypeSerializer>(InternetMediaTypes.HalJson);

            // Register each defined client within the configuration settings with the RequestClientFactory.
            foreach (ClientSettings clientSettings in factorySettings.Clients)
            {
                IRequestSettings requestSettings = BuildRequestSettings(clientSettings);
                RequestClientFactory.Instance.RegisterBaseAddress(clientSettings.BaseAddress, requestSettings);
            }
        }

        private IRequestSettings BuildRequestSettings(ClientSettings settings)
        {
            // Create new request settings instance to be populated
            // from the defined configuration settings.
            var requestSettings = RequestSettings.Create();

            if (!string.IsNullOrWhiteSpace(settings.AcceptType))
            {
                requestSettings.Headers.AcceptMediaType(settings.AcceptType);
            }

            if (!string.IsNullOrWhiteSpace(settings.ContentType))
            {
                requestSettings.Headers.ContentMediaType(settings.ContentType);
            }

            if (settings.UseHalDefaults)
            {
                requestSettings.UseHalDefaults();                
            }

            if (settings.Headers != null)
            {
                foreach (var headerKeyValue in settings.Headers)
                {
                    requestSettings.Headers.Add(headerKeyValue.Key, headerKeyValue.Value);
                }
            }

            return requestSettings;
        }

        private void LazyLoadClientEntryPoints(ClientFactorySettings factorySettings)
        {
            // Local method called when entry resource is lazy loaded.
            HalEntryPointResource GetEntry(ClientSettings clientSettings)
            {
                var client = RequestClientFactory.Instance.GetClient(clientSettings.BaseAddress);
                var request = ApiRequest.Get(clientSettings.EntryPointUrl);
                var errorMsg = "Could not load entry resource at {EntryAddress} for base address {BaseAddress}";

                HalEntryPointResource entryPoint = null;
                try
                {
                    var response = client.Send<HalEntryPointResource>(request).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Context.Logger.LogError(LogEvents.EntryPoint, errorMsg, 
                            clientSettings.EntryPointUrl, 
                            clientSettings.BaseAddress);
                    }

                    entryPoint = response.Content;
                }
                catch (Exception ex)
                {
                    Context.Logger.LogError(LogEvents.EntryPoint, ex, errorMsg,
                        clientSettings.EntryPointUrl,
                        clientSettings.BaseAddress);
                }

                return entryPoint;
            }

            var clientsWithEntryPoints = factorySettings.Clients
               .Where(c => !string.IsNullOrWhiteSpace(c.EntryPointUrl));

            foreach (ClientSettings clientSettings in clientsWithEntryPoints)
            {
                _entryPointMappings[clientSettings.BaseAddress] = new Lazy<HalEntryPointResource>(
                    () => GetEntry(clientSettings));
            }
        }

        public string GetBaseAddressForName(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name not specified.", nameof(clientName));

            if (!_addressNameMappings.TryGetValue(clientName, out string baseAddress))
            {
                throw new InvalidCastException(
                    $"Base address not been register for the client name: {clientName}");
            }

            return baseAddress;
        }

        public HalEntryPointResource GetEntryPointResource(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name not specified.", nameof(clientName));

            string baseAddress = GetBaseAddressForName(clientName);

            if (!_entryPointMappings.TryGetValue(baseAddress, out Lazy<HalEntryPointResource> entryResource))
            {
                throw new InvalidCastException(
                    $"Entry Point Resource not register for the client name: {clientName}");
            }

            return entryResource.Value;
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose && !_disposed)
            {
                RequestClientFactory.Instance.Dispose();

                _disposed = true;
            }

            base.Dispose(dispose);
        }
    }
}
