using System;
using System.Collections.Generic;

namespace NetFusion.WebApi.Metadata
{
    /// <summary>
    /// Metadata about a given controller that exposes routes.
    /// </summary>
    public class EndpointMetadata
    {
        internal Type ControllerType { get; set; }

        /// <summary>
        /// The name assigned to the endpoint.  Will be the WebApi assigned name 
        /// or the overridden name specified with the EndpointMetadataName attribute.
        /// </summary>
        public string EndpointName { get; set; }

        /// <summary>
        /// Meta-data for each of the controller's exposed routes.  By default, a route 
        /// is not included in the returned meta-data unless the EndpointData attribute
        /// is specified with the IncludeAllRoutes set to True.
        /// </summary>
        public IDictionary<string, RouteMetadata> RouteMetadata { get; set; }
    }
}
