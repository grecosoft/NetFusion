using System;
using System.Collections.Generic;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;

namespace NetFusion.Rest.Server.Resources
{
	/// <summary>
	/// Extension methods that can be invoked on a resource supporting
	/// the IHalResource interface.
	/// </summary>
    public static class HalResourceExtensions
    {
        /// <summary>
		/// Embeds a resource within parent resource.
		/// </summary>
		/// /// <param name="resource">Resource supporting IHalResource.</param>
		/// <param name="embeddedResource">The resource to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void Embed(this IHalResource resource, IHalResource embeddedResource, string named = null)
		{
			EmbedValue(resource, embeddedResource, named);
		}

        /// <summary>
        /// Embeds a model within parent resource.
        /// </summary>
        /// <param name="resource">Resource supporting IHalResource.</param>
        /// <param name="model">The object to embed.</param>
        /// <param name="named">The name used to identify the embedded object.</param>
        public static void Embed(this IHalResource resource, object model, string named)
		{
			if (model == null) throw new ArgumentNullException(nameof(model),
				"Value to embed cannot be null.");
			
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(named));

			EmbedValue(resource, model, named);
		}

		/// <summary>
		/// Embeds a collection of resources within parent resource.
		/// </summary>
		/// <typeparam name="T">The type of each resource in the collection.</typeparam>
		/// <param name="resource">Resource supporting IHalResource.</param>
		/// <param name="embeddedResources">The collection of resources to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public static void Embed<T>(this IHalResource resource, IEnumerable<T> embeddedResources, string named = null)
			where T : class, IHalResource
		{
            if (embeddedResources == null) throw new ArgumentNullException(nameof(embeddedResources),
                "Enumeration of resources cannot be null.");

			EmbedValue(resource, new ResourceCollection<T>(embeddedResources), named);
		}

		/// <summary>
		/// Embeds a collection of models within parent resource.
		/// </summary>
		/// <param name="resource">Resource supporting IHalResource.</param>
		/// <param name="models">The objects to be embedded.</param>
		/// <param name="named">Name used to identify the embedded objects.</param>
		public static void Embed(this IHalResource resource, IEnumerable<object> models, string named)
		{
			if (models == null) throw new ArgumentNullException(nameof(models));
			
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(named));

			EmbedValue(resource, models, named);
		}

		private static void EmbedValue(IHalResource resource, object value, string named = null)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource));
			
			if (value == null) throw new ArgumentNullException(nameof(value), 
				"Value to embed cannot be null.");
			
			string embeddedName = named ?? GetResourceEmbeddedName(value);
            if (embeddedName == null)
            {
                throw new InvalidOperationException(
                    $"The embedded name for type: {value.GetType().FullName} could not be determined.  " +
                    $"The name was not provided and its type was not decorated with the attribute: {typeof(ExposedResourceNameAttribute).FullName}");
            }

            resource.Embedded ??= new Dictionary<string, object>();

            if (resource.Embedded.ContainsKey(embeddedName))
            {
                throw new InvalidOperationException(
                    $"The resource already has an embedded value named: {embeddedName}.");
            }

            resource.Embedded[embeddedName] = value;
		}

		private static string GetResourceEmbeddedName(object resource) =>
			resource.GetAttribute<ExposedResourceNameAttribute>()?.ResourceName;
    }
}