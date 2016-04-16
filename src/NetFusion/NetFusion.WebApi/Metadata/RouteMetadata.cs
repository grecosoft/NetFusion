using System.Collections.Generic;

namespace NetFusion.WebApi.Metadata
{
    /// <summary>
    /// Metadata for a route assocated with an endpoint.
    /// </summary>
    public class RouteMetadata
    {
        public string Template { get; set; }
        public IEnumerable<string> Methods { get; set; }
        public IEnumerable<ParameterMetadata> ParamMetadata { get; set; }
    }
}
