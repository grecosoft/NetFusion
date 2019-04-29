using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings.Plugin;

namespace NetFusion.Settings
{
    /// <summary>
    /// Adds the types specific to the settings plug-in.  The extension
    /// methods contained within this class are used only for unit-testing.
    /// The mock plug-in classes contained within NetFusion.Test implement
    /// IPluginTypeAccessor.
    /// </summary>
    public static class PluginTypeAccessorExtensions
    {
        /// <summary>
        /// Adds the needed plug-in types specific to the plug-in.
        /// </summary>
        /// <param name="plugin">Mock plug-in instance.</param>
        /// <returns>Reference to the plug-in for method chaining.</returns>
        public static IPluginTypeAccessor UseSettingsPlugin(this IPluginTypeAccessor plugin)
        {
            plugin.AddPluginType<AppSettingsModule>();

            return plugin;
        }
    }
}


