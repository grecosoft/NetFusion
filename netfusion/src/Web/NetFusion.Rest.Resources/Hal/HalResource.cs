using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Base class implementing the IHalResource interface from which resources can be derived.
    /// </summary>
    public class HalResource : IHalResource
    {
        /// <summary>
        /// List of links populated based on the configured resource metadata.
        /// </summary>
        public IDictionary<string, Link> Links { get; set; }

        /// <summary>
        /// Named embedded resources.
        /// </summary>
        public IDictionary<string, IResource> Embedded { get; set; }

		/// <summary>
		/// Embeds a resource within parent resource.
		/// </summary>
		/// <param name="embeddedResource">The resource to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public void Embed(IHalResource embeddedResource, string named = null)
		{
            if (embeddedResource == null) throw new ArgumentNullException(nameof(embeddedResource), 
                "Resource to embed cannot be null.");

            EmbedResource(embeddedResource, named);
		}

		/// <summary>
		/// Embeds an object within parent resource.
		/// </summary>
		/// <param name="value">The object to embed.</param>
		/// <param name="named">The name used to identify the embedded object.</param>
		public void Embed(object value, string named)
		{
			if (value == null) throw new ArgumentNullException(nameof(value),
				"Value to embed cannot be null.");
			
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(named));

			EmbedResource(new ObjectResource(value), named);
		}

		/// <summary>
		/// Embeds a collection of resources.
		/// </summary>
		/// <typeparam name="T">The type of each resource in the collection.</typeparam>
		/// <param name="embeddedResources">The collection of resources to embed.</param>
		/// <param name="named">Optional name used to identity the embedded resource.</param>
		public void Embed<T>(IEnumerable<T> embeddedResources, string named = null)
			where T : class, IHalResource
		{
            if (embeddedResources == null) throw new ArgumentNullException(nameof(embeddedResources),
                "Enumeration of resources cannot be null.");

			EmbedResource(new ResourceCollection<T>(embeddedResources), named);
		}

		/// <summary>
		/// Embeds a collection of objects.
		/// </summary>
		/// <param name="values">The objects to be embedded.</param>
		/// <param name="named">Name used to identify the embedded objects.</param>
		public void Embed(IEnumerable<object> values, string named)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));
			
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(named));
			
			var wrappedObjs = values.Select(v => new ObjectResource(v));
			
			EmbedResource(new ResourceCollection<ObjectResource>(wrappedObjs), named);
		}

		private void EmbedResource(IResource embeddedResource, string named = null)
		{
            string embeddedName = named ?? GetResourceEmbeddedName(embeddedResource);
            if (embeddedName == null)
            {
                throw new InvalidOperationException(
                    $"The embedded name for resource type: {embeddedResource.GetType().FullName} could not be determined.  " +
                    $"The name was not provided and its type was not decorated with the attribute: {typeof(ExposedResourceNameAttribute).FullName}");
            }

            Embedded ??= new Dictionary<string, IResource>();

            if (Embedded.ContainsKey(embeddedName))
            {
                throw new InvalidOperationException(
                    $"The resource of type: {GetType().FullName} already has an embedded resource named: {embeddedName}.");
            }

            Embedded[embeddedName] = embeddedResource;
		}

		private static string GetResourceEmbeddedName(IResource resource) =>
			resource.GetAttribute<ExposedResourceNameAttribute>()?.ResourceName;
    }
}
