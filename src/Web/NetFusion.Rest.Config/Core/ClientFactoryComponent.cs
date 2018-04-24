using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Config.Modules;
using System;

namespace NetFusion.Rest.Config
{
    /// <summary>
    /// Service class that delegates to the RequestClientFactory so consumers
    /// can inject IRequestClientFactory into service/adapter components.
    /// </summary>
    public class ClientFactoryComponent : IRequestClientFactory
    {
        public readonly IClientFactoryModule _factoryModule;

        public ClientFactoryComponent(IClientFactoryModule factoryModule)
        {
            _factoryModule = factoryModule;
        }

        // Maps the configured client name stored in the configuration to 
        // its associated base address and delegates to the client factory
        // to obtain associated client instance.
        public IRequestClient GetClient(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name not specified.", nameof(clientName));

            string baseAddress = _factoryModule.GetBaseAddressForName(clientName);
            return RequestClientFactory.Instance.GetClient(baseAddress);
        }

        public HalEntryPointResource GetEntryPoint(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name not specified.", nameof(clientName));

            return _factoryModule.GetEntryPointResource(clientName);
        }
    }
}
