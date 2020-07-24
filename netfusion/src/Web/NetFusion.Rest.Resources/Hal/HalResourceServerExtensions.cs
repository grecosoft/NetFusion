using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Rest.Resources.Hal
{
	/// <summary>
	/// HalResource extension methods used on server side of an WebApi implementation.
	/// The methods are used to wrap API response models into new HalResource or
	/// HalResource collections.  Also provides methods for embedding child resources
	/// and models into parent resource.
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
	    /// Wraps a list of models into an array of resources.
	    /// </summary>
	    /// <param name="models">The list of models.</param>
	    /// <typeparam name="TModel">The type of the model.</typeparam>
	    /// <returns>List of resources wrapping the list of models.</returns>
	    public static HalResource<TModel>[] AsResources<TModel>(this IEnumerable<TModel> models)
			where TModel: class
	    {
		    return models.Select(m => m.AsResource()).ToArray();
	    }
	    
        /// <summary>
		/// Embeds a resource within parent resource.
		/// </summary>
		/// <param name="resource">Parent resource to embed child resource.</param>
		/// <param name="embeddedResource">The resource to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void EmbedResource<TModel>(this HalResource resource,
	        HalResource<TModel> embeddedResource, 
	        string named) where TModel: class
		{
			EmbedValue<TModel>(resource, embeddedResource, named);
		}

        /// <summary>
        /// Embeds a model within parent resource.
        /// </summary>
        /// <param name="resource">Parent resource to embed child model.</param>
        /// <param name="model">The model to embed.</param>
        /// <param name="named">The name used to identify the embedded model.</param>
        public static void EmbedModel<TModel>(this HalResource resource, TModel model, string named)
			where TModel: class
		{
			EmbedValue<TModel>(resource, model, named);
		}

		/// <summary>
		/// Embeds a collection of resources within parent resource.
		/// </summary>
		/// <typeparam name="TModel">The type of each resource's model in the collection.</typeparam>
		/// <param name="resource">Parent resource to embed child resources.</param>
		/// <param name="embeddedResources">The collection of resources to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource collection..</param>
		public static void EmbedResources<TModel>(this HalResource resource, 
			IEnumerable<HalResource<TModel>> embeddedResources, string named)
			where TModel : class
		{
			EmbedValue<TModel>(resource, embeddedResources, named);
		}

		/// <summary>
		/// Embeds a collection of models within parent resource.
		/// </summary>
		/// <param name="resource">Parent resource s.</param>
		/// <param name="models">The model to be embedded.</param>
		/// <param name="named">Name used to identify the embedded models.</param>
		public static void EmbedModels<TModel>(this HalResource resource, 
			IEnumerable<TModel> models, string named) where TModel: class
		{
			EmbedValue<TModel>(resource, models, named);
		}

		private static void EmbedValue<TModel>(HalResource resource, object value, string named)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource), 
				"The parent HAL resource to embed child cannot be null.");
			
			if (value == null) throw new ArgumentNullException(nameof(value), 
				"The child value to embed cannot be null.");

			if (named == null)
			{
				throw new InvalidOperationException(
					$"The embedded name for type: {typeof(TModel).FullName} was not specified.");
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