using NetFusion.Rest.Server.Meta;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Used to store metadata associated with resources for a specific media type.
    /// </summary>
    public class MediaTypeEntry 
    {
        /// <summary>
        /// The media-type specified with the metadata.
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// The provider responsible for applying the metadata to the resource.
        /// </summary>
        public IResourceProvider Provider { get; }

        // Metadata for each resource type configured for this media-type.
        private readonly Dictionary<Type, IResourceMeta> _resourceTypeMeta;

        public MediaTypeEntry(string mediaType, IResourceProvider provider)
        {
            _resourceTypeMeta = new Dictionary<Type, IResourceMeta>();

            MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Returns the metadata associated with a resource.
        /// </summary>
        /// <returns>The resource metadata and boolean flag if found.</returns>
        /// <param name="resourceType">The resource type.</param>
        public (IResourceMeta meta, bool ok) GetResourceTypeMeta(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType),
                "Resource type not specified.");

            bool isFound = _resourceTypeMeta.TryGetValue(resourceType, out IResourceMeta resourceMeta);
            return (resourceMeta, isFound);
        }

        /// <summary>
        /// Adds the resource metadata associated with a specific resource type.
        /// </summary>
        /// <param name="resourceMeta">The resource metadata.</param>
        public void AddResourceMeta(IResourceMeta resourceMeta)
        {
            if (resourceMeta == null) throw new ArgumentNullException(nameof(resourceMeta),
                "Resource metadata cannot be null.");

            if (_resourceTypeMeta.ContainsKey(resourceMeta.ResourceType)) 
            {
                throw new InvalidOperationException(
                    $"Resource metadata already specified for resource type: {resourceMeta.ResourceType.FullName}");
            }

            _resourceTypeMeta[resourceMeta.ResourceType] = resourceMeta;
        }
    }
}
