namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Responsible for creating IRestClient instances.
    /// </summary>
    public interface IRestClientFactory
    {
        /// <summary>
        /// Creates an IRestClient instance configured by the client application.
        /// </summary>
        /// <param name="name">The name of the configured HttpClient.</param>
        /// <returns>Instance of REST Client providing additional functionality
        /// delegating to an inner HttpClient.</returns>
        IRestClient CreateClient(string name);
    }
}