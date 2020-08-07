using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Mappings;

namespace NetFusion.Rest.Server.Plugin
{
    /// <summary>
    /// Module interface defined my the main plug-in module called during the
    /// bootstrap process to initialize any metadata information needed 
    /// during application execution.
    /// </summary>
    public interface IResourceMediaModule : IPluginModuleService
    {
        /// <summary>
        /// Applies the resource metadata for a given media-type.
        /// </summary>
        /// <returns>True if the resource metadata was applied.  Otherwise, False is returned.</returns>
        /// <param name="mediaType">The media type of the metadata to be applied to the resource.</param>
        /// <param name="context">Contains details about the current request.</param>
        bool ApplyResourceMeta(string mediaType, ResourceContext context);

        /// <summary>
        /// Returns a media-type entry for the specified media-type. 
        /// </summary>
        /// <param name="mediaType">The media type of the metadata to retrieve.</param>
        /// <returns>Object containing metadata required to generate
        /// links associated with resources.</returns>
        (MediaTypeEntry entry, bool ok) GetMediaTypeEntry(string mediaType);
    }
}
