using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.MongoDB.Plugin.Modules;
using NetFusion.Settings.Plugin;

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
        /// <summary>
        /// Adds a plugin to the composite container providing services to query MongoDb.
        /// </summary>
        /// <param name="composite">Reference to the composite container builder.</param>
        /// <returns>Reference to the composite container builder.</returns>
        public static ICompositeContainerBuilder AddMongoDb(this ICompositeContainerBuilder composite)
        {
            // Add dependent plugins:
            composite.AddSettings();
            
            // Add MongoDB plugin:
            composite.AddPlugin<MongoDbPlugin>();
            return composite;
        }
    }
}