using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// 
    /// </summary>
    public class HalResource : IHalResource
    {
        /// <summary>
        /// Untyped reference to the model.  Only set on
        /// the server.  Will be null on the client.
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
            ModelValue = modelValue;
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

        public static HalResource<TModel> ForModel<TModel>(TModel model)
            where TModel: class
        {
            return new HalResource<TModel>(model);
        }
    }
    
    /// <summary>
    /// Provides an implementation of the IHalResource wrapping a model used to
    /// associate links and embedded resources.
    /// </summary>
    public class HalResource<TModel> : HalResource
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
