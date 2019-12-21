using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Provides an implementation of the IHalResource wrapping a model used to
    /// associate links and embedded resources.
    /// </summary>
    public class HalResource : IHalResource
    {
        /// <summary>
        /// The model associated with the resource.
        /// </summary>
        public object Model { get; }

        protected HalResource()
        {
            
        }
        
        /// <summary>
        /// Constructor to wrap model with HAL resource information.
        /// </summary>
        /// <param name="model">The model state.</param>
        public HalResource(object model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
        
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
