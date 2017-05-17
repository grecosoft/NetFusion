using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Modules;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Adds the types specific to the messaging plug-in.  The extension
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
        public static IPluginTypeAccessor UseMessagingPlugin(this IPluginTypeAccessor plugin)
        {
            plugin
                .AddPluginType<MessagingModule>()
                .AddPluginType<MessagingConfig>();

            return plugin;
        }
    }
}
