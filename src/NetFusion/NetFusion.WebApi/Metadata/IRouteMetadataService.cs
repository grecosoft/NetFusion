using System.Collections.Generic;
using System.Web.Http;

namespace NetFusion.WebApi.Metadata
{
    /// <summary>
    /// Service that exposes WebApi route metadata that can be used by client to invoke
    /// endpoints routes without have the URL hard-coded in their code.  Each WebApi
    /// controller specifies the EndpointMetadata attribute with the name the endpoint.
    /// </summary>
    public interface IRouteMetadataService
    {
        /// <summary>
        /// Returns metadata for all WebApi controllers found in all host assemblies.
        /// </summary>
        /// <returns>Dictionary keyed by endpoint name containing metadata about the
        /// endpoint and it associated routes.</returns>
        IDictionary<string, EndpointMetadata> GetApiMetadata();

        /// <summary>
        /// Returns metadata for a specific WebApi controller found in the host assemblies.
        /// </summary>
        /// /// <typeparam name="T">The controller typw.</typeparam>
        /// <returns>Dictionary keyed by endpoint name containing metadata about the
        /// endpoint and it associated routes.</returns>
        IDictionary<string, EndpointMetadata> GetApiMetadata<T>()
            where T : ApiController;


        /// <summary>
        /// Returns metadata for all WebApi controllers found in all host assemblies
        /// belonging the the same namespace.
        /// </summary>
        /// <typeparam name="T">The controller type used to determine the namespace.</typeparam>
        /// <returns>Dictionary keyed by endpoint name containing metadata about the
        /// endpoint and it associated routes.</returns>
        IDictionary<string, EndpointMetadata> GetApiMetadataInNamespace<T>()
            where T : ApiController;
    }
}
