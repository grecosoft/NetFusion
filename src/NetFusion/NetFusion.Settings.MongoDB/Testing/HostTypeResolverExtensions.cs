using NetFusion.Bootstrap.Testing;
using NetFusion.Common;
using NetFusion.MongoDB.Testing;
using NetFusion.Settings.MongoDB.Configs;
using NetFusion.Settings.MongoDB.Modules;

namespace NetFusion.Settings.MongoDB.Testing
{
    /// <summary>
    /// Adds a mock core plug-in with the needed plug-in types required to
    /// bootstrap the MongoDB Settings plug-in.
    /// </summary>
    public static class HostTypeResolverExtensions
    {
        /// <summary>
        /// Adds core plug-in with required Settings MongoDB types.
        /// </summary>
        /// <param name="resolver">The test type resolver</param>
        public static void AddMongoSettingsPlugin(this TestTypeResolver resolver)
        {
            Check.NotNull(resolver, nameof(resolver), "resolver not specified");

            resolver.AddMongoDbPlugin();

            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MongoSettingsModule>()
                .AddPluginType<AppSettingMapping>()
                .AddPluginType<MongoAppSettingsConfig>()
                .AddPluginType<MongoAppSettingsConfigSection>();
        }
    }
}
