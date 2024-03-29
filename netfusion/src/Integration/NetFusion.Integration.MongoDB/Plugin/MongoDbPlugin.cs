using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Core.Settings.Plugin;
using NetFusion.Integration.MongoDB.Plugin.Modules;

namespace NetFusion.Integration.MongoDB.Plugin;

public class MongoDbPlugin : PluginBase
{
    public override string PluginId => "4BE391F9-F687-4E49-90A3-D38300E3A751";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: MongoDB";

    public MongoDbPlugin()
    {
        AddModule<MongoModule>();
        AddModule<MappingModule>();
            
        SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Integration.MongoDB";
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
        return composite.AddSettings()
            .AddPlugin<MongoDbPlugin>();
    }
}