using System.Threading.Tasks;
using NetFusion.Rest.Client.Resources;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Implemented by the RequestClientBuilder and passed to the
    /// built IRequestClient instance to provide a callback used
    /// to load the request client's associated entry resource.
    /// </summary>
    public interface IServiceEntryApiProvider
    {
        /// <summary>
        /// Returns the associated service api entry resource.  The same
        /// instance is returned after the first successful load.
        /// </summary>
        /// <returns>Entry point resource.</returns>
        Task<HalEntryPointResource> GetEntryPointResource();
    }
}
