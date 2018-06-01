using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.Server.Modules
{
    public static class PluginTypeAccessorExtensions
    {
        /// <summary>
        /// Adds the needed plug-in types specific to the plug-in.  This is used
        /// when configuring an NetFusion AppContainer for unit-testing.
        /// </summary>
        /// <param name="plugin">Mock plug-in instance.</param>
        /// <returns>Reference to the plug-in for method chaining.</returns>
        public static IPluginTypeAccessor UseResourcePlugin(this IPluginTypeAccessor plugin)
        {
            plugin
                .AddPluginType<RestModule>()
                .AddPluginType<ResourceMediaModule>();

            return plugin;
        }
    }
}
