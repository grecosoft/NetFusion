using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Implements the IHalResource interface containing a non-type model.
    /// </summary>
    public abstract class HalResource : IHalResource
    {
        /// <summary>
        /// Untyped reference to the model.  Only set on the server.
        /// Will be null on the client.
        /// </summary>
        public object ModelValue { get; private set; }

        /// <summary>
        /// Required for Deserialization on the Client.
        /// </summary>
        protected HalResource()
        {
            
        }
        
        /// <summary>
        /// Constructor to initialize model.
        /// </summary>
        /// <param name="modelValue">The untyped model associated with the resource.</param>
        protected HalResource(object modelValue)
        {
            ModelValue = modelValue ?? throw new ArgumentNullException(nameof(modelValue));
        }
        
        /// <summary>
        /// Links associated with the resource used to take actions on the resource or
        /// navigate to other related resources.
        /// </summary>
        [JsonPropertyName("_links")]
        public IDictionary<string, Link> Links { get; set; }
    
        /// <summary>
        /// Associated resources that are embedded by the server and returned
        /// to the client.  Each embedded resources is identified by key.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public IDictionary<string, object> Embedded { get; set; }
    }
    
    /// <summary>
    /// Provides an implementation of the IHalResource wrapping a typed model
    /// used to associate links and embedded resources.
    /// </summary>
    public class HalResource<TModel> : HalResource, IHalResource<TModel>
        where TModel: class
    {
        /// <summary>
        /// The model associated with the resource.
        /// </summary>
        public TModel Model { get; set; }

        /// <summary>
        /// Required for Deserialization on the Client.
        /// </summary>
        public HalResource()
        {
            
        }

        /// <summary>
        /// Constructor to wrap model with HAL resource information.
        /// </summary>
        /// <param name="model">The model state.</param>
        public HalResource(TModel model) : base(model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}
