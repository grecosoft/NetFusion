using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Rest.Resources.Hal
{
	/// <summary>
	/// Extension methods that can be invoked on a resource supporting the IHalResource interface.
	/// These extension methods are called by the server-side API methods that build the resources
	/// to be returned to the client.  NOTE:  The NetFusion.Rest.Client NuGet contains extension
	/// methods that are specific to .NET based clients.
	/// </summary>
    public static class HalResourceServerExtensions
    {
	    /// <summary>
	    /// Wraps a model within a HalResource instance.  The returned resource can then
	    /// have associated links and embedded resources.
	    /// </summary>
	    /// <param name="model">The model to wrap with HAL specific information such
	    /// as links and embedded resources.</param>
	    /// <typeparam name="TModel">The type of the associated model.</typeparam>
	    /// <returns>Instance of resource wrapping the model.</returns>
	    public static HalResource<TModel> AsResource<TModel>(this TModel model)
		    where TModel: class
	    {
		    return new HalResource<TModel>(model);
	    }

	    /// <summary>
	    /// Wraps an enumeration of models into an array of resources.
	    /// </summary>
	    /// <param name="models">The list of models.</param>
	    /// <typeparam name="TModel">The type of the model.</typeparam>
	    /// <returns>List of resources wrapping the enumeration of models.</returns>
	    public static HalResource<TModel>[] AsResources<TModel>(this IEnumerable<TModel> models)
			where TModel: class
	    {
		    return models.Select(m => m.AsResource()).ToArray();
	    }
	    
        /// <summary>
		/// Embeds a resource within parent resource.
		/// </summary>
		/// /// <param name="resource">Parent resource supporting IHalResource.</param>
		/// <param name="embeddedResource">The resource to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void EmbedResource(this HalResource resource, HalResource embeddedResource, string named = null)
		{
			named ??= embeddedResource.ModelValue.GetType().GetExposedResourceTypeName();
			EmbedValue(resource, embeddedResource, named);
		}

        /// <summary>
        /// Embeds a non-resource object within parent resource.
        /// </summary>
        /// <param name="resource">Parent resource supporting IHalResource.</param>
        /// <param name="model">The object to embed.</param>
        /// <param name="named">The name used to identify the embedded object.</param>
        public static void EmbedModel<TModel>(this HalResource resource, TModel model, string named = null)
			where TModel: class
		{
			named ??= model.GetType().GetExposedResourceTypeName();
			EmbedValue(resource, model, named);
		}

		/// <summary>
		/// Embeds a collection of resources within parent resource.
		/// </summary>
		/// <typeparam name="TModel">The type of each resource in the collection.</typeparam>
		/// <param name="resource">Parent resource supporting IHalResource.</param>
		/// <param name="embeddedResources">The collection of resources to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void EmbedResources<TModel>(this HalResource resource, IEnumerable<HalResource<TModel>> embeddedResources, string named = null)
			where TModel : class
		{
			named ??= typeof(TModel).GetExposedResourceTypeName();
			EmbedValue(resource, embeddedResources, named);
		}

		/// <summary>
		/// Embeds a collection of non-resource models within parent resource.
		/// </summary>
		/// <param name="resource">Parent resource supporting IHalResource.</param>
		/// <param name="models">The objects to be embedded.</param>
		/// <param name="named">Name used to identify the embedded objects.</param>
		public static void EmbedModels<TModel>(this HalResource resource, IEnumerable<TModel> models, string named = null)
			where TModel: class
		{
			named ??= typeof(TModel).GetExposedResourceTypeName();
			EmbedValue(resource, models, named);
		}

		private static void EmbedValue(HalResource resource, object value, string named)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource), 
				"The parent HAL resource to embed child cannot be null.");
			
			if (value == null) throw new ArgumentNullException(nameof(value), 
				"The child value to embed cannot be null.");

			if (named == null)
            {
                throw new InvalidOperationException(
                    $"The embedded name for type: {value.GetType().FullName} could not be determined.  " +
                    "The name was not explicitly provided and the model type was not decorated with the attribute: " + 
                    $"{typeof(ExposedNameAttribute).FullName}");
            }

            resource.Embedded ??= new Dictionary<string, object>();

            if (resource.Embedded.ContainsKey(named))
            {
                throw new InvalidOperationException(
                    $"The resource already has an embedded value named: {named}.");
            }

            resource.Embedded[named] = value;
		}
    }
}