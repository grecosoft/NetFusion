using NetFusion.Bootstrap.Plugins;
using NetFusion.MongoDB.Plugin;
using NetFusion.MongoDB.Settings;

namespace NetFusion.MongoDB
{
    /// <summary>
    /// Adds the types specific to the MongoDb plug-in.  The extension
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
        public static IPluginTypeAccessor UseMongoDbPlugin(this IPluginTypeAccessor plugin)
        {
            plugin
                .AddPluginType<MappingModule>()
                .AddPluginType<MongoModule>()
                .AddPluginType<MongoSettings>();

            return plugin;
        }
    }
}
