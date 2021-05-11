using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Implements the IResource interface containing a non-type model.
    /// This interface is used internally when attaching the HAL specific
    /// information to a returned HAL Resource.
    /// </summary>
    public class HalResource : IResource
    {
        /// <summary>
        /// Untyped reference to the model.  Only set on the server.
        /// </summary>
        [JsonIgnore]
        public object ModelValue { get; }

        /// <summary>
        /// Required for Deserialization on the Client.
        /// </summary>
        public HalResource()
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
        /// Creates a new HAL Resource with no associated state to which other
        /// resources and response models can be embedded.
        /// <param name="instance">Delegate passed instance of created resource.</param>
        /// <returns>The created resource without an associated model.</returns>
        /// </summary>
        public static HalResource New(Action<HalResource> instance = null)
        {
            var resource = new HalResource();
            instance?.Invoke(resource);

            return resource;
        }

        /// <summary>
        /// Creates a new HAL Resource warping a model.
        /// </summary>
        /// <param name="model">The model to be wrapped inside resource.</param>
        /// <param name="instance">Delegate passed instance of created resource.</param>
        /// <typeparam name="TModel">The type of the associated model.</typeparam>
        /// <returns>The created resource wrapping the model.</returns>
        public static HalResource<TModel> New<TModel>(TModel model, Action<HalResource<TModel>> instance = null)
            where TModel : class
        {
            var resource = new HalResource<TModel>(model);
            instance?.Invoke(resource);

            return resource;
        }

        /// <summary>
        /// Links associated with the resource used to take actions on the resource or
        /// navigate to other related resources.
        /// </summary>
        [JsonPropertyName("_links")]
        public IDictionary<string, Link> Links { get; set; }
    
        /// <summary>
        /// Associated resources and/or models that are embedded by the server and
        /// returned to the client.  Each embedded member is identified by key.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public IDictionary<string, object> Embedded { get; set; }
    }
    
    /// <summary>
    /// Provides an implementation of the IResource wrapping a typed model
    /// used to associate links and embedded resources.
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
