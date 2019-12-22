using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Client.Resources
{
    /// <summary>
    /// Base resource class from which application resources are derived.  
    /// Also allows the consumer of the resource to obtain any embedded 
    /// named resources.
    /// </summary>
    public class HalResource<TModel> : IHalResource<TModel>
    {
        /// <summary>
        /// Embedded related named resources.
        /// </summary>
        [JsonPropertyName("_embedded")]
		public IDictionary<string, object> Embedded { get; set; }

        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        [JsonPropertyName("_links")]
        public IDictionary<string, Link> Links { get; set; }
        
        public TModel Model { get; set; }
    }
}
