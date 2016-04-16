using NetFusion.Bootstrap.Testing;
using NetFusion.Settings.Configs;
using NetFusion.Settings.Modules;

namespace NetFusion.Settings.Testing
{
    public static class HostTypeResolverExtensions
    {
        public static void AddSettingsPlugin(this HostTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<AppSettingsModule>()
                .AddPluginType<NetFusionConfig>()
                .AddPluginType<NetFusionConfigSection>();
        }
    }
}
