using NetFusion.Rest.Server.Meta;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Used to store metadata associated with resources for a specific media-type.
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
        /// Returns the metadata associated with a source type.
        /// </summary>
        /// <param name="sourceType">The type to determine if there is associated metadata.</param>
        /// <returns>The resource metadata and boolean flag if found.</returns>
        public (IResourceMeta meta, bool ok) GetResourceTypeMeta(Type sourceType)
        {
            if (sourceType == null) throw new ArgumentNullException(nameof(sourceType),
                "Source type not specified.");

            bool isFound = _resourceTypeMeta.TryGetValue(sourceType, out IResourceMeta resourceMeta);
            return (resourceMeta, isFound);
        }

        /// <summary>
        /// Adds the resource metadata associated with a source type.
        /// </summary>
        /// <param name="resourceMeta">The resource metadata.</param>
        public void AddResourceMeta(IResourceMeta resourceMeta)
        {
            if (resourceMeta == null) throw new ArgumentNullException(nameof(resourceMeta),
                "Resource metadata cannot be null.");

            if (_resourceTypeMeta.ContainsKey(resourceMeta.SourceType)) 
            {
                throw new InvalidOperationException(
                    $"Resource metadata already specified for source type: {resourceMeta.SourceType.FullName} " + 
                    $"For media-type: {MediaType}");
            }

            _resourceTypeMeta[resourceMeta.SourceType] = resourceMeta;
        }
    }
}
