using Autofac;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using NetFusion.Rest.Config.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Rest.Config.Modules
{
    /// <summary>
    /// Plugin-Module responsible for reading request client application settings and configuring the RequestClientFactory 
    /// instance.  A given configuration can specify an entry-point URL used to describe initial URL called to return resources 
    /// managed by the service.  After this point, the client navigates using links relations associated with the returned resource.
    /// </summary>
    public class ClientFactoryModule : PluginModule,
        IClientFactoryModule
    {
        private bool _disposed;

        private Dictionary<string, string> _addressNameMappings;                // SimpleName ==> BaseUrl
        private Dictionary<string, HalEntryPointResource> _entryPointMappings;  // BaseUrl ==> EntryPoint

        public ClientFactoryModule()
        {
            _entryPointMappings = new Dictionary<string, HalEntryPointResource>();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register the component that can be injected into application components
            // and used to make HTTP calls.
            builder.RegisterType<ClientFactoryComponent>()
                .As<IRequestClientFactory>()
                .SingleInstance();
        }

        public override void StartModule(IContainer container, ILifetimeScope scope)
        {
            var factorySettings = scope.Resolve<ClientFactorySettings>();
            if (factorySettings == null || factorySettings.Clients == null)
            {
                // No registered HTTP configurations.
                return; 
            }

            CreateAddressNameMapping(factorySettings);
            RegisterClientsWithFactory(factorySettings);
            LoadClientEntryPoints(factorySettings);
        }        

        // Create a mapping between a simple name to identify an endpoint and the base URL.
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

        // Creates new request settings populated from the defined configuration settings.
        private IRequestSettings BuildRequestSettings(ClientSettings settings)
        {
            var requestSettings = RequestSettings.Create();

            if (settings.AcceptTypes != null)
            {
                foreach(AcceptType acceptType in settings.AcceptTypes)
                {
                    requestSettings.Headers.AcceptMediaType(acceptType.Accept, acceptType.Quality);
                }
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

        // For each service API with a configured entry point, invoke the entry-point URL and cache 
        // the entry-point resource.  The entry-point resource contains template URLs used to load 
        // initial resources after which links returned directly on resources are used for navigation.  
        // This is a common pattern providing an entry into an API.
        private void LoadClientEntryPoints(ClientFactorySettings factorySettings)
        {
            HalEntryPointResource GetEntry(ClientSettings clientSettings)
            {
                var client = RequestClientFactory.Instance.GetClient(clientSettings.BaseAddress);
                var request = ApiRequest.Get(clientSettings.EntryPointUrl);
                var errorMsg = "Could not load entry resource at {EntryAddress} for base address {BaseAddress}";

                HalEntryPointResource entryPoint = null;
                try
                {
                    var response = client.SendAsync<HalEntryPointResource>(request).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Context.Logger.LogError(RestConfigLogEvents.REST_CONFIG_EXCEPTION, errorMsg, 
                            clientSettings.EntryPointUrl, 
                            clientSettings.BaseAddress);
                    }

                    entryPoint = response.Content;
                }
                catch (Exception ex)
                {
                    Context.Logger.LogError(RestConfigLogEvents.REST_CONFIG_EXCEPTION, ex, errorMsg,
                        clientSettings.EntryPointUrl,
                        clientSettings.BaseAddress);
                }

                return entryPoint;
            }

            var clientsWithEntryPoints = factorySettings.Clients
               .Where(c => !string.IsNullOrWhiteSpace(c.EntryPointUrl));

            foreach (ClientSettings clientSettings in clientsWithEntryPoints)
            {
                _entryPointMappings[clientSettings.BaseAddress] = GetEntry(clientSettings);
            }
        }

        public string GetBaseAddressForName(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name not specified.", nameof(clientName));

            if (!_addressNameMappings.TryGetValue(clientName, out string baseAddress))
            {
                throw new InvalidOperationException(
                    $"Base address not been register for the client name: {clientName}");
            }

            return baseAddress;
        }

        public HalEntryPointResource GetEntryPointResource(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name not specified.", nameof(clientName));

            string baseAddress = GetBaseAddressForName(clientName);

            if (!_entryPointMappings.TryGetValue(baseAddress, out HalEntryPointResource entryResource))
            {
                throw new InvalidCastException(
                    $"Entry Point Resource not register for the client name: {clientName}");
            }

            return entryResource;
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
