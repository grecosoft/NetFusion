using Newtonsoft.Json;
using System.Collections.Generic;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Implemented by a resource that can be associated with links.
    /// </summary>
    public interface ILinkedResource
    {
        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        [JsonProperty(PropertyName = "_links", Order = -1)]
        IDictionary<string, Link> Links { get; set; }
    }
}
