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
public class ResourceContext(
    HttpContext httpContext,
    IResource resource,
    IResourceMediaModule mediaModule,
    IApiMetadataService apiMetadata,
    IUrlHelper urlHelper,
    ILogger logger)
{
    public HttpContext HttpContext { get; } = httpContext;

    // Services that can be utilized when adding the metadata
    // to the resource instance.
    public IResourceMediaModule MediaModule { get; } = mediaModule;
    public IApiMetadataService ApiMetadata { get; } = apiMetadata;
    public IUrlHelper UrlHelper { get; } = urlHelper;
    public ILogger Logger { get; } = logger;

    // Instance of a resource being returned.
    public IResource Resource { get; internal set; } = resource;

    // Optional reference to the state associated with the resource.
    // Based on the media-type, this value may not be set.
    public object? Model { get; set; }

    // The metadata associated with the source type.
    public IResourceMeta? Meta { get; set; }
}