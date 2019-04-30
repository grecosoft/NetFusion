using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.EntityFramework.Plugin.Modules;
using NetFusion.Settings.Plugin;

namespace NetFusion.EntityFramework.Plugin
{
    public class EntityFrameworkPlugin : PluginBase
    {
        public override string PluginId => "4316A9C70-C3AE-4DC2-8DEA-097EBDB342F7";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion Roslyn Plug-in";

        public EntityFrameworkPlugin()
        {
            AddModule<EntityContextModule>();
            
            Description = "Plugin providing bootstrapping and extensions to EntityFramework Core.";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddEntityFramework(this ICompositeContainerBuilder composite)
        {
            // Add dependent plugins:
            composite.AddSettings();
            
            // Add Entity Framework Plugin:
            composite.AddPlugin<EntityFrameworkPlugin>();
            return composite;
        }
    }
}