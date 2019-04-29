using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Roslyn.Plugin
{
    public class RoslynPlugin : Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "4316A9C70-C3AE-4DC2-8DEA-097EBDB342F7";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion Roslyn Plug-in";

        public RoslynPlugin()
        {
            
            AddModule<ExpressionModule>();
            
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Common/NetFusion.Roslyn";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.domain.roslyn.overview";
            
            Description = "Plug-in that provides domain entity Roslyn based implementations.  This includes " +
                          "the runtime execution of expressions against domain entities and it set of optional " +
                          "dynamic attributes.";
        }

    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddRoslyn(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<RoslynPlugin>();
            return composite;
        }
    }
}