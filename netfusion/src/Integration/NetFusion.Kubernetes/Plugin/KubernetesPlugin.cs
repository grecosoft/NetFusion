using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Kubernetes.Plugin
{
    public class KubernetesPlugin : PluginBase
    {
        public override string PluginId => "45F260FF8-18C9-42A4-8303-4BCC96909AC6";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: Kubernetes";

        public KubernetesPlugin()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Kubernetes";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        /// <summary>
        /// Adds a plugin to the composite container providing services to query MongoDb.
        /// </summary>
        /// <param name="composite">Reference to the composite container builder.</param>
        /// <returns>Reference to the composite container builder.</returns>
        public static ICompositeContainerBuilder AddKubernetes(this ICompositeContainerBuilder composite)
        {
            return composite.AddPlugin<KubernetesPlugin>();
        }
    }
}