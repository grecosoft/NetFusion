using System.Diagnostics.CodeAnalysis;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.Server.Mappings;

namespace NetFusion.Web.Rest.Server.Plugin;

/// <summary>
/// Module interface defined by the main plug-in module called during the
/// bootstrap process to initialize any metadata information needed 
/// during application execution.
/// </summary>
public interface IResourceMediaModule : IPluginModuleService
{
    /// <summary>
    /// Returns a media-type entry for the specified media-type. 
    /// </summary>
    /// <param name="mediaType">The media type of the metadata to retrieve.</param>
    /// <param name="entry">Reference to the media-type entry if registered.</param>
    /// <returns>True if entry found for specified meta-type.</returns>
    bool TryGetMediaTypeEntry(string mediaType, [NotNullWhen(true)] out MediaTypeEntry? entry);
    
    /// <summary>
    /// Applies the resource metadata for a given media-type.
    /// </summary>
    /// <returns>True if the resource metadata was applied.  Otherwise, False is returned.</returns>
    /// <param name="mediaType">The media type of the metadata to be applied to the resource.</param>
    /// <param name="context">Contains details about the current request.</param>
    bool ApplyResourceMeta(string mediaType, ResourceContext context);
}