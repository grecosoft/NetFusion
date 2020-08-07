using System;
using System.Collections.Concurrent;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Provides caching an other high-level services.
    /// </summary>
    public class RestClientService : IRestClientService
    {
        private readonly ConcurrentDictionary<string, IRestClient> _restClients;
        private readonly IRestClientFactory _clientFactory;

        public RestClientService(IRestClientFactory clientFactory)
        {
            _restClients = new ConcurrentDictionary<string, IRestClient>();
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public IRestClient GetClient(string name)
        {
            return _restClients.GetOrAdd(name, _clientFactory.CreateClient);
        }
    }
}