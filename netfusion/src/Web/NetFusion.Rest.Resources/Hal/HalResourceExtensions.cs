using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Rest.Resources.Hal
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
			EmbedResource(resource, embeddedResource, named);
		}

        /// <summary>
        /// Embeds an object within parent resource.
        /// </summary>
        /// <param name="resource">Resource supporting IHalResource.</param>
        /// <param name="value">The object to embed.</param>
        /// <param name="named">The name used to identify the embedded object.</param>
        public static void Embed(this IHalResource resource, object value, string named)
		{
			if (value == null) throw new ArgumentNullException(nameof(value),
				"Value to embed cannot be null.");
			
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(named));

			EmbedResource(resource, new ObjectResource(value), named);
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

			EmbedResource(resource, new ResourceCollection<T>(embeddedResources), named);
		}

		/// <summary>
		/// Embeds a collection of objects within parent resource.
		/// </summary>
		/// <param name="resource">Resource supporting IHalResource.</param>
		/// <param name="values">The objects to be embedded.</param>
		/// <param name="named">Name used to identify the embedded objects.</param>
		public static void Embed(this IHalResource resource, IEnumerable<object> values, string named)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));
			
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(named));
			
			var wrappedObjs = values.Select(v => new ObjectResource(v));
			
			EmbedResource(resource, new ResourceCollection<ObjectResource>(wrappedObjs), named);
		}

		private static void EmbedResource(IHalResource resource, IResource embeddedResource, string named = null)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource));
			
			if (embeddedResource == null) throw new ArgumentNullException(nameof(embeddedResource), 
				"Resource to embed cannot be null.");
			
			string embeddedName = named ?? GetResourceEmbeddedName(embeddedResource);
            if (embeddedName == null)
            {
                throw new InvalidOperationException(
                    $"The embedded name for resource type: {embeddedResource.GetType().FullName} could not be determined.  " +
                    $"The name was not provided and its type was not decorated with the attribute: {typeof(ExposedResourceNameAttribute).FullName}");
            }

            resource.Embedded ??= new Dictionary<string, IResource>();

            if (resource.Embedded.ContainsKey(embeddedName))
            {
                throw new InvalidOperationException(
                    $"The resource of type: {resource.GetType().FullName} already has an embedded resource named: {embeddedName}.");
            }

            resource.Embedded[embeddedName] = embeddedResource;
		}

		private static string GetResourceEmbeddedName(IResource resource) =>
			resource.GetAttribute<ExposedResourceNameAttribute>()?.ResourceName;
    }
}