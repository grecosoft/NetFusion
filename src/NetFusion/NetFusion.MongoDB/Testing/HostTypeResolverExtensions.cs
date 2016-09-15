using NetFusion.Bootstrap.Testing;
using NetFusion.MongoDB.Modules;

namespace NetFusion.MongoDB.Testing
{
    /// <summary>
    /// Adds a mock core plug-in with the needed plug-in types required to
    /// bootstrap the MongoDB plug-in.
    /// </summary>
    public static class HostTypeResolverExtensions
    {
        /// <summary>
        /// Adds core plug-in with required MongoDB types.
        /// </summary>
        /// <param name="resolver">The test type resolver</param>
        public static void AddMongoDbPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MappingModule>()
                .AddPluginType<MongoModule>();
        }
    }
}
