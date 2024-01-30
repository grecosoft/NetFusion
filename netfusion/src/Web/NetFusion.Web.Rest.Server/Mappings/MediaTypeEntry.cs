using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NetFusion.Web.Rest.Server.Meta;

namespace NetFusion.Web.Rest.Server.Mappings;

/// <summary>
/// Used to store metadata associated with resources for a specific media-type.
/// </summary>
public class MediaTypeEntry(string mediaType, IResourceProvider provider)
{
    /// <summary>
    /// The media-type specified with the metadata.
    /// </summary>
    public string MediaType { get; } = mediaType ?? throw new ArgumentNullException(nameof(mediaType));

    /// <summary>
    /// The provider responsible for applying the metadata to the resource.
    /// </summary>
    public IResourceProvider Provider { get; } = provider ?? throw new ArgumentNullException(nameof(provider));

    // Metadata for each resource type configured for this media-type.
    private readonly Dictionary<Type, IResourceMeta> _resourceTypeMeta = new();

    /// <summary>
    /// Returns the metadata associated with a source type.
    /// </summary>
    /// <param name="sourceType">The type to determine if there is associated metadata.</param>
    /// <param name="meta">Resource metadata associated with the source type.</param>
    /// <returns>True if the source type has metadata.  Otherwise, false.</returns>
    public bool TryGetResourceTypeMeta(Type sourceType, [NotNullWhen(true)]out IResourceMeta? meta)
    {
        if (sourceType == null) throw new ArgumentNullException(nameof(sourceType),
            "Source type not specified.");

        return _resourceTypeMeta.TryGetValue(sourceType, out meta);
    }

    /// <summary>
    /// Adds the resource metadata associated with a source type.
    /// </summary>
    /// <param name="resourceMeta">The resource metadata.</param>
    public void AddResourceMeta(IResourceMeta resourceMeta)
    {
        if (resourceMeta == null) throw new ArgumentNullException(nameof(resourceMeta),
            "Resource metadata cannot be null.");

        if (!_resourceTypeMeta.TryAdd(resourceMeta.SourceType, resourceMeta)) 
        {
            throw new InvalidOperationException(
                $"Resource metadata already specified for source type: {resourceMeta.SourceType.FullName} " + 
                $"For media-type: {MediaType}");
        }
    }
}