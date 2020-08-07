namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Provides a service that delegates to IRequestClientFactory and provides
    /// caching of the underlying HttpClient and other higher level methods.
    /// </summary>
    public interface IRestClientService
    {
        /// <summary>
        /// Returns a reference to a configured named client for making
        /// HTTP requests.  
        /// </summary>
        /// <param name="name">The name of the configured client.</param>
        /// <returns>Reference to the request client.  The same instance
        /// of the IRestClient is returned based on name.</returns>
        IRestClient GetClient(string name);
    }
}