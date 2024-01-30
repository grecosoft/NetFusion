using System;
using System.Collections.Concurrent;

namespace NetFusion.Web.Rest.Client.Core;

/// <summary>
/// Provides caching an other high-level services.
/// </summary>
public class RestClientService(IRestClientFactory clientFactory) : IRestClientService
{
    private readonly ConcurrentDictionary<string, IRestClient> _restClients = new();
    
    private readonly IRestClientFactory _clientFactory = clientFactory ?? 
        throw new ArgumentNullException(nameof(clientFactory));

    public IRestClient GetClient(string name)
    {
        return _restClients.GetOrAdd(name, _clientFactory.CreateClient);
    }
}