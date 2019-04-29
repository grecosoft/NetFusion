using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Meta;

namespace NetFusion.Rest.Server.Plugin
{
    /// <summary>
    /// Module called during the bootstrap process that resolves all resource-metadata associated classes
    /// and caches the information to be used during application execution.
    /// 
    /// http://stateless.co/hal_specification.html
    /// https://www.iana.org/assignments/link-relations/link-relations.xhtml#link-relations-1
    /// </summary>

    public class ResourceMediaModule : PluginModule, IResourceMediaModule
    {
        private IEnumerable<IResourceMap> ResourceMappings { get; set; }

        // MediaTypeName --> Entry
        private readonly Dictionary<string, MediaTypeEntry> _mediaResourceTypeMeta = new Dictionary<string, MediaTypeEntry>();

        // ResourceType --> ExternalTypeName
        private Dictionary<Type, string> _namedResourceModels;

        // Caches the name associated with all resources decorated with the NamedResource attribute.
        // This eliminates having to execute reflective code during application execution.
        public override void Initialize()
        {          
            _namedResourceModels = Context.AllPluginTypes
                .Where(pt => pt.HasAttribute<NamedResourceAttribute>())
                .ToDictionary(pt => pt, 
                    pt => pt.GetAttribute<NamedResourceAttribute>().ResourceName);
        }

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

                // Build the mapping.
                resourceMap.BuildMap();
            
                foreach (IResourceMeta resourceMeta in resourceMap.ResourceMeta)
                {
                    mediaTypeEntry.AddResourceMeta(resourceMeta);
                }
            }
        }

        private IResourceProvider CreateProvider(IResourceMap resourceMap)
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
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));

            bool isFound = _mediaResourceTypeMeta.TryGetValue(mediaType, out MediaTypeEntry mediaTypeEntry);
            return (mediaTypeEntry, isFound);
        }    

        public IResourceMeta GetRequestedResourceMediaMeta(
            IHeaderDictionary headers,
            Type resourceType)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
   
            if (! headers.TryGetValue(HeaderNames.Accept, out StringValues values))
            {
                return null;
            }

            // Determine the media type to used by finding the first one, ordered by importance,
            // for which there is a configure metadata.
            var mediaType = values.Select(v => MediaTypeHeaderValue.Parse(v))
                .OrderByDescending(mt => mt.Quality)
                .FirstOrDefault(mt => _mediaResourceTypeMeta.ContainsKey(mt.MediaType.ToString()))?.MediaType;

            if (mediaType == null)
            {
                return null;
            }

			var entryResult = GetMediaTypeEntry(mediaType.ToString());
			var metaResult = entryResult.entry.GetResourceTypeMeta(resourceType);

            return metaResult.meta;
        }

        public string GetMappedResourceName(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType),
                "Resource type cannot be null.");

            _namedResourceModels.TryGetValue(resourceType, out string resourceName);
            return resourceName;
        }

        public bool ApplyResourceMeta(string mediaType, ResourceContext context)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Media type not specified.", nameof(mediaType));

            if (context == null) throw new ArgumentNullException(nameof(context));

            var entryResult = GetMediaTypeEntry(mediaType);
            if (entryResult.ok)
            {
                var metaResult = entryResult.entry.GetResourceTypeMeta(context.Resource.GetType());
                if (!metaResult.ok)
                {
                    return false;
                }

                context.Meta = metaResult.meta;
                entryResult.entry.Provider.ApplyResourceMeta(context);
            }

            return true;
        }
    }
}
