using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.MongoDB.Plugin.Modules;

namespace NetFusion.MongoDB.Plugin
{
    public class MongoDbPlugin : PluginBase
    {
        public override string PluginId => "4BE391F9-F687-4E49-90A3-D38300E3A751";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "MongoDb Plug-in";

        public MongoDbPlugin()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.MongoDB";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.mongodb.overview";
            
            // Modules:
            AddModule<MongoModule>();
            AddModule<MappingModule>();
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddMongoDb(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<MongoDbPlugin>();
            return composite;
        }
    }
}