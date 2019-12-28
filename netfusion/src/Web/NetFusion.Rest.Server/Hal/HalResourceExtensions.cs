using System;
using System.Collections.Generic;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Resources;

namespace NetFusion.Rest.Server.Hal
{
	/// <summary>
	/// Extension methods that can be invoked on a resource supporting the IHalResource interface.
	/// These extension methods are called by the server-side API methods that build the resources
	/// to be returned to the client.  NOTE:  The NetFusion.Rest.Client NuGet contains extension
	/// methods that are specific to .NET based clients.
	/// </summary>
    public static class HalResourceExtensions
    {
	    /// <summary>
	    /// Wraps a model within a HalResource instance.  The returned resource can then
	    /// have associated links and embedded resources.
	    /// </summary>
	    /// <param name="model">The model to wrap with HAL specific information such
	    /// as links and embedded resources.</param>
	    /// <typeparam name="TModel">The type of the associated model.</typeparam>
	    /// <returns>Instance of resource wrapping the model.</returns>
	    public static IHalResource<TModel> AsResource<TModel>(this TModel model)
		    where TModel: class
	    {
		    return new HalResource<TModel>(model);
	    }
	    
        /// <summary>
		/// Embeds a resource within parent resource.
		/// </summary>
		/// /// <param name="resource">Parent resource supporting IHalResource.</param>
		/// <param name="embeddedResource">The resource to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void Embed(this IHalResource resource, IHalResource embeddedResource, string named = null)
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
        public static void Embed(this IHalResource resource, object model, string named = null)
		{
			named ??= model.GetType().GetExposedResourceTypeName();
			EmbedValue(resource, model, named);
		}

		/// <summary>
		/// Embeds a collection of resources within parent resource.
		/// </summary>
		/// <typeparam name="T">The type of each resource in the collection.</typeparam>
		/// <param name="resource">Parent resource supporting IHalResource.</param>
		/// <param name="embeddedResources">The collection of resources to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void Embed<T>(this IHalResource resource, IEnumerable<T> embeddedResources, string named)
			where T : class, IHalResource
		{
			if (named == null) throw new ArgumentNullException(nameof(named), 
				"The name associated with the embedded resource collection must be specified");
			
			EmbedValue(resource, new ResourceCollection<T>(embeddedResources), named);
		}

		/// <summary>
		/// Embeds a collection of non-resource models within parent resource.
		/// </summary>
		/// <param name="resource">Parent resource supporting IHalResource.</param>
		/// <param name="models">The objects to be embedded.</param>
		/// <param name="named">Name used to identify the embedded objects.</param>
		public static void Embed(this IHalResource resource, IEnumerable<object> models, string named)
		{
			if (named == null) throw new ArgumentNullException(nameof(named), 
				"The name associated with the embedded resource collection must be specified");
			
			EmbedValue(resource, models, named);
		}

		private static void EmbedValue(IHalResource resource, object value, string named)
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
                    $"{typeof(ExposedResourceNameAttribute).FullName}");
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