using NetFusion.Bootstrap.Plugins;
using NetFusion.Mapping.Modules;

namespace NetFusion.Mapping
{
    public static class PluginTypeAccessorExtensions
    {
        /// <summary>
        /// Adds the needed plug-in types specific to the plug-in.
        /// </summary>
        /// <param name="plugin">Mock plug-in instance.</param>
        /// <returns>Reference to the plug-in for method chaining.</returns>
        public static IPluginTypeAccessor UseMappingPlugin(this IPluginTypeAccessor plugin)
        {
            plugin
                .AddPluginType<MappingModule>();

            return plugin;
        }
    }
}