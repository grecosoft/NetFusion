using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.Server.Mappings;
using NetFusion.Web.Rest.Server.Meta;

namespace NetFusion.Web.Rest.Server.Plugin.Modules;

/// <summary>
/// Module called during the bootstrap process that resolves all resource-metadata associated classes
/// and caches the information to be used during microservice execution.  Based on the request specified
/// Accept header, this cached metadata is used by the associated IOutputFormatter to add information to
/// returned resources.
/// </summary>
public class ResourceMetaModule : PluginModule, IResourceMediaModule
{
    // Set by Bootstrapper:
    private IEnumerable<IResourceMap> ResourceMappings { get; set; } = Enumerable.Empty<IResourceMap>();

    // MediaTypeName (i.e. application/hal+json) --> Entry
    private readonly Dictionary<string, MediaTypeEntry> _mediaResourceTypeMeta = new();

    // Caches all of the resource-metadata associated with resources for a specific media type.
    public override void Configure()
    {
        foreach (IResourceMap resourceMap in ResourceMappings)
        {
            if (resourceMap.MediaType == null)
            {
                throw new InvalidOperationException(
                    $"The resource map of type: {resourceMap.GetType()} didn't specify the media-type.");
            }

            AddMediaResourceTypeMeta(resourceMap);
        }
    }

    private void AddMediaResourceTypeMeta(IResourceMap resourceMap)
    {
        // Create an entry for the media-type name.  Each media-type will have a single entry.
        if (! _mediaResourceTypeMeta.TryGetValue(resourceMap.MediaType, out MediaTypeEntry? value))
        {
            value = new MediaTypeEntry(
                resourceMap.MediaType, 
                CreateProvider(resourceMap));
            
            _mediaResourceTypeMeta[resourceMap.MediaType] = value;
        }

        MediaTypeEntry mediaTypeEntry = value;
        resourceMap.BuildMap();
            
        foreach (IResourceMeta resourceMeta in resourceMap.ResourceMeta)
        {
            mediaTypeEntry.AddResourceMeta(resourceMeta);
        }
    }

    // A IResourceProvider implementation is responsible for applying the resource-metadata 
    // to resources returned when the request has an Accept value (i.e. application/hal+json)
    // indicating that the returned resource should be augmented.
    private static IResourceProvider CreateProvider(IResourceMap resourceMap) => 
        (IResourceProvider)resourceMap.ProviderType.CreateInstance();


    public bool TryGetMediaTypeEntry(string mediaType, [NotNullWhen(true)]out MediaTypeEntry? entry)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
            throw new ArgumentException("Media type not specified.", nameof(mediaType));
        
        return _mediaResourceTypeMeta.TryGetValue(mediaType, out entry);
    }

    // Determines if there is metadata associated with the resource for a 
    // given media-type.  If found, the metadata is set on the context and 
    // passed to the associated provider.
    public bool ApplyResourceMeta(string mediaType, ResourceContext context)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
            throw new ArgumentException("Media type not specified.", nameof(mediaType));

        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!TryGetMediaTypeEntry(mediaType, out MediaTypeEntry? entry))
        {
            return false;
        }
        
        // Determines the type of the object on which the metadata is based.
        var metaSourceType = (context.Model ?? context.Resource).GetType();

        if (!entry.TryGetResourceTypeMeta(metaSourceType, out IResourceMeta? meta))
        {
            return false;
        }

        context.Meta = meta;
        entry.Provider.ApplyResourceMeta(context);
        return true;
    }
}