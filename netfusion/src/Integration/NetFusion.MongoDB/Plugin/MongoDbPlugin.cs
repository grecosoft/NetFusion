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
            AddModule<MongoModule>();
            AddModule<MappingModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.MongoDB";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.mongodb.overview";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddMongoDb(this ICompositeContainerBuilder composite)
        {
            composite.AddPlugin<MongoDbPlugin>();
            return composite;
        }
    }
}