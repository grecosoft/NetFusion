using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetFusion.Rest.Client.Resources
{
    /// <summary>
    /// Base REST resource.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        [JsonProperty(PropertyName = "_links")]
        public Dictionary<string, Link> Links { get; set; }

        // When submitting resources back to the server for updating or 
        // other use-cases, the links should not be serialized.
        public bool ShouldSerializeLinks() => false;
    }
}
