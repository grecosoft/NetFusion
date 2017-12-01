using Microsoft.AspNetCore.Http;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Meta;
using System;

namespace NetFusion.Rest.Server.Modules
{
    /// <summary>
    /// Module interface defined my the main plug-in module called during the
    /// bootstrap process to initialize any REST based information needed during
    /// application execution.
    /// </summary>
    public interface IResourceMediaModule : IPluginModuleService
    {     
        /// <summary>
        /// Returns the string name associated with a given resource type.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <returns>The mapped resource name.</returns>
        string GetMappedResourceName(Type resourceType);

        /// <summary>
        /// Returns the resource metadata based on the media type specified in the request header.
        /// </summary>
        /// <param name="headers">The request headers.</param>
        /// <param name="resourceType">The resource type.</param>
        /// <returns>Resource metadata for associated resource type.</returns>
        IResourceMeta GetRequestedResourceMediaMeta(IHeaderDictionary headers, Type resourceType);

        /// <summary>
        /// Applies the resource metadata to a specific resource for a given media-type.
        /// </summary>
        /// <returns>True if the resource metadata was applied.  Otherwise, False is returned.</returns>
        /// <param name="mediaType">The media type of the metadata to be applied to the resource.</param>
        /// <param name="context">Contains details about the current request.</param>
        bool ApplyResourceMeta(string mediaType, ResourceContext context);
    }
}
