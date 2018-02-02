using NetFusion.Web.Mvc.Composite.Models;

namespace NetFusion.Web.Mvc.Composite
{
    /// <summary>
    /// Service registered by a plug-in module that is delegated to
    /// from routes added to the MVC pipeline to return information
    /// written to the composite log by the bootstrap process.
    /// </summary>
    public interface ICompositeService
    {
        /// <summary>
        /// Returns the overall structure of the application and the
        /// plugins from which it was composed.
        /// </summary>
        /// <returns>Created Model.</returns>
        CompositeStructure GetStructure();

        /// <summary>
        /// Returns the details for a specific plugin module from
        /// which the application was build.
        /// </summary>
        /// <param name="pluginId">The key value of the plugin.</param>
        /// <returns>Created Model.</returns>
        PluginDetails GetPluginDetails(string pluginId);
    }
}
