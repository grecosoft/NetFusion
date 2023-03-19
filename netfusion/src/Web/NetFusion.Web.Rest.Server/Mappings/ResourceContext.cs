using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Resources;
using NetFusion.Web.Rest.Server.Meta;
using NetFusion.Web.Rest.Server.Plugin;

namespace NetFusion.Web.Rest.Server.Mappings;

/// <summary>
/// Context class containing information for the resource and services
/// used by a provider when applying media-type specific information to
/// returned resources.
/// </summary>
public class ResourceContext
{
    public HttpContext HttpContext { get; } 
        
    // Services that can be utilized when adding the metadata
    // to the resource instance.
    public IResourceMediaModule MediaModule { get; } 
    public IApiMetadataService ApiMetadata { get; }
    public IUrlHelper UrlHelper { get; }
    public ILogger Logger { get; }

    public ResourceContext(HttpContext httpContext, 
        IResource resource,
        IResourceMediaModule mediaModule,
        IApiMetadataService apiMetadata, 
        IUrlHelper urlHelper,
        ILogger logger)
    {
        Resource = resource;
        HttpContext = httpContext;
        MediaModule = mediaModule;
        ApiMetadata = apiMetadata;
        UrlHelper = urlHelper;
        Logger = logger;
    }

    // Instance of a resource being returned.
    public IResource Resource { get; internal set; }

    // Optional reference to the state associated with the resource.
    // Based on the media-type, this value may not be set.
    public object? Model { get; set; }

    // The metadata associated with the source type.
    public IResourceMeta? Meta { get; set; }
}