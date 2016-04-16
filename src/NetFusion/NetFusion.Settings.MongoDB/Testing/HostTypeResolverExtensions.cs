using NetFusion.Bootstrap.Testing;
using NetFusion.Common;
using NetFusion.MongoDB.Testing;
using NetFusion.Settings.Mongo.Configs;
using NetFusion.Settings.Mongo.Modules;

namespace NetFusion.Settings.Mongo.Testing
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
