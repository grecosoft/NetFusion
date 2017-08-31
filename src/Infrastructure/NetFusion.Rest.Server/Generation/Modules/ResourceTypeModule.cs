using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Generation.Core;

namespace NetFusion.Rest.Server.Generation.Modules
{
    public class ResourceTypeModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<ResourceTypeReader>()
                .As<IResourceTypeReader>()
                .InstancePerLifetimeScope();
        }
    }
}
