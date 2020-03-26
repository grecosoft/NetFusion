using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Meta;

namespace NetFusion.Rest.Server.Plugin.Modules
{
    /// <summary>
    /// Module called during the bootstrap process that resolves all resource-metadata associated classes
    /// and caches the information to be used during application execution.  Based on the request specified
    /// Accept header, this cached metadata is used by the associated IOutputFormatter to add information to
    /// returned resources.
    /// </summary>
    public class ResourceMetaModule : PluginModule, IResourceMediaModule
    {
        // IKnownPluginType - set by NetFusion bootstrapper.  
        private IEnumerable<IResourceMap> ResourceMappings { get; set; }

        // MediaTypeName (i.e. HAL) --> Entry
        private readonly Dictionary<string, MediaTypeEntry> _mediaResourceTypeMeta = new Dictionary<string, MediaTypeEntry>();

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

                // Create an entry for the media-type name.  Each media-type will have a single entry.
                if (! _mediaResourceTypeMeta.TryGetValue(resourceMap.MediaType, out MediaTypeEntry mediaTypeEntry))
                {
                    IResourceProvider provider = CreateProvider(resourceMap);

                    mediaTypeEntry = new MediaTypeEntry(resourceMap.MediaType, provider);
                    _mediaResourceTypeMeta[mediaTypeEntry.MediaType] = mediaTypeEntry;
                }

                // Build and add the configured resource metadata to the media-type entry.
                resourceMap.BuildMap();
            
                foreach (IResourceMeta resourceMeta in resourceMap.ResourceMeta)
                {
                    mediaTypeEntry.AddResourceMeta(resourceMeta);
                }
            }
        }

        // A IResourceProvider implementation is responsible for applying the resource-metadata 
        // to resources returned when the request has an Accept value (i.e. application/hal+json)
        // indicating that the returned resource should be augmented.
        private static IResourceProvider CreateProvider(IResourceMap resourceMap)
        {
            if (resourceMap.ProviderType == null)
            {
                throw new InvalidOperationException(
                    $"The resource map of type: {resourceMap.GetType()} did not set the provider type.");
            }

            return (IResourceProvider)resourceMap.ProviderType.CreateInstance();
        }

        private (MediaTypeEntry entry, bool ok) GetMediaTypeEntry(string mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Media type not specified.", nameof(mediaType));

            bool isFound = _mediaResourceTypeMeta.TryGetValue(mediaType, out MediaTypeEntry mediaTypeEntry);
            return (mediaTypeEntry, isFound);
        }    
        
        // Determines if there is metadata associated with the resource for a 
        // given media-type.  If found, the metadata is set on the context and 
        // passed to the associated provider.
        public bool ApplyResourceMeta(string mediaType, ResourceContext context)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Media type not specified.", nameof(mediaType));

            if (context == null) throw new ArgumentNullException(nameof(context));

            var (entry, ok) = GetMediaTypeEntry(mediaType);
            if (! ok)
            {
                return false;
            }
            
            // Determines the type of the object on which the metadata is based.
            var metaSourceType = (context.Model ?? context.Resource).GetType();
            
            var (meta, hasMeta) = entry.GetResourceTypeMeta(metaSourceType);
            if (! hasMeta)
            {
                return false;
            }

            context.Meta = meta;
            entry.Provider.ApplyResourceMeta(context);
            return true;
        }
    }
}
