using NetFusion.Bootstrap.Testing;
using NetFusion.Settings.Configs;
using NetFusion.Settings.Modules;

namespace NetFusion.Settings.Testing
{
    /// <summary>
    /// Adds a mock core plug-in with the needed plug-in types required to
    /// bootstrap the settings plug-in.
    /// </summary>
    public static class HostTypeResolverExtensions
    {
        /// <summary>
        /// Adds core plug-in with required settings types.
        /// </summary>
        /// <param name="resolver">The test type resolver</param>
        public static void AddSettingsPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<AppSettingsModule>()
                .AddPluginType<NetFusionConfig>()
                .AddPluginType<NetFusionConfigSection>();
        }
    }
}
