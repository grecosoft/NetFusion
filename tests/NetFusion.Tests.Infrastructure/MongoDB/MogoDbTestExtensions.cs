using NetFusion.Bootstrap.Testing;
using NetFusion.MongoDB.Modules;
using NetFusion.Settings.Testing;
using NetFusion.Tests.MongoDB.Mocks;

namespace NetFusion.Tests.MongoDB
{
    public static class MogoDbTestExtensions
    {
        public static void SetupMogoDbPlugIn(this TestTypeResolver resolver)
        {
            // Configure dependent modules:
            resolver.AddSettingsPlugin();

            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MappingModule>()
                .AddPluginType<MockMongoModule>();
        }

        public static void SetupValidMongoConsumingPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppComponentPlugin>()    
                .AddPluginType<MockMongoDb>()   // Database settings
                .AddPluginType<MockEntity>()    // Database entity
                .AddPluginType<MockEntityClassMap>();   // Entity mapping
        }
    }
}
