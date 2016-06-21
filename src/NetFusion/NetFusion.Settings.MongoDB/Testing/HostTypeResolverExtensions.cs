using NetFusion.Bootstrap.Testing;
using NetFusion.Common;
using NetFusion.MongoDB.Testing;
using NetFusion.Settings.MongoDB.Configs;
using NetFusion.Settings.MongoDB.Modules;

namespace NetFusion.Settings.MongoDB.Testing
{
    public static class HostTypeResolverExtensions
    {
        public static void AddMongoSettingsPlugin(this HostTypeResolver resolver)
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
