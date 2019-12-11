using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Base class implementing the IHalResource interface from which resources can be derived.
    /// </summary>
    public class HalResource : IHalResource
    {
        /// <summary>
        /// List of links populated based on the configured resource metadata.
        /// </summary>
        [JsonPropertyName("_links")]
        public IDictionary<string, Link> Links { get; set; }
    
        /// <summary>
        /// Named embedded resources.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public IDictionary<string, object> Embedded { get; set; }
    }
}
