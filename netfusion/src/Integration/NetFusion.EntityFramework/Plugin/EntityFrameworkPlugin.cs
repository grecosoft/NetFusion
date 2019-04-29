using NetFusion.Bootstrap.Refactors;

namespace NetFusion.EntityFramework.Plugin
{
    public class EntityFrameworkPlugin : PluginDefinition
    {
        public override string PluginId => "4316A9C70-C3AE-4DC2-8DEA-097EBDB342F7";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.CorePlugin;
        public override string Name => "NetFusion Roslyn Plug-in";

        public EntityFrameworkPlugin()
        {
            AddModule<EntityContextModule>();
            
            Description = "Plugin providing bootstrapping and extensions to EntityFramework Core.";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddEntityFramework(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<EntityFrameworkPlugin>();
            return composite;
        }
    }
}