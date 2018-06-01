using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;

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

		private void EmbedResource(IResource embeddedResource, string named = null)
		{
            string embeddedName = named ?? GetResourceEmbeddedName(embeddedResource);
            if (embeddedName == null)
            {
                throw new InvalidOperationException(
                    $"The embedded name for resource type: {embeddedResource.GetType().FullName} could not be determined.  " +
                    $"The name was not provided and its type was not decorated with the attribute: {typeof(NamedResourceAttribute).FullName}");
            }

            Embedded = Embedded ?? new Dictionary<string, IResource>();

            if (Embedded.ContainsKey(embeddedName))
            {
                throw new InvalidOperationException(
                    $"The resource of type: {GetType().FullName} already has an embedded resource named: {embeddedName}.");
            }

            Embedded[embeddedName] = embeddedResource;
		}
        
        private static string GetResourceEmbeddedName(IResource resource)
        {
            var namedResource = resource.GetAttribute<NamedResourceAttribute>();
            return namedResource?.ResourceName;
        }
    }
}
