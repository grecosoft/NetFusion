using NetFusion.Bootstrap.Refactors;

namespace NetFusion.Mapping.Plugin
{
    public class MappingPlugin : PluginDefinition
    {
        public override string PluginId => "83C90E78-D245-4B0D-A4FC-E74B11227766";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.CorePlugin;
        public override string Name => "Mapping Plug-in";

        public MappingPlugin()
        {
            AddModule<MappingModule>();
            
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Common/NetFusion.Mapping";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.mapping.overview";
            
            Description =  "Plug-In used to map object instances to other object representations.  This plug-in " +
                           "configures and coordinates the mapping process but does not dependent on any specific " +
                           "mapping library.  The host application can use an open-source mapping library of choice.";
        }

    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddMapping(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<MappingPlugin>();
            return composite;
        }
    }
}