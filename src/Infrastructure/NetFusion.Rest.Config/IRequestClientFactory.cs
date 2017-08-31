using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Resources;

namespace NetFusion.Rest.Config
{
    /// <summary>
    /// Provides services for obtaining configured request-clients for
    /// making external HTTP requests to external service providers.
    /// </summary>
    public interface IRequestClientFactory
    {
        /// <summary>
        /// Returns the request-client configured for the host application.
        /// </summary>
        /// <param name="clientName">The name of the client specified within
        /// the host application's configuration.</param>
        /// <returns>Corresponding request client.</returns>
        IRequestClient GetClient(string clientName);

        /// <summary>
        /// Returns the entry-point resource configured for the host application.
        /// </summary>
        /// <param name="clientName">The name of the client specified within the host
        /// application's configuration.</param>
        /// <returns>Corresponding entry-point resource.</returns>
        HalEntryPointResource GetEntryPoint(string clientName);
    }
}
