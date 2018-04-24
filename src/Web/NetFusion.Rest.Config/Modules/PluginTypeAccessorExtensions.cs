using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.Config.Modules
{
    public static class PluginTypeAccessorExtensions
    {
        public static IPluginTypeAccessor UseRestConfigPlugin(this IPluginTypeAccessor plugin)
        {
            plugin
                .AddPluginType<ClientFactoryModule>();

            return plugin;
        }
    }
}
