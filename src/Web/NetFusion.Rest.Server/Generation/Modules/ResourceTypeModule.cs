using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Generation.Core;

namespace NetFusion.Rest.Server.Generation.Modules
{
    public class ResourceTypeModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register service used to read TypeScript files corresponding
            // to resourced returned by the API.
            builder.RegisterType<ResourceTypeReader>()
                .As<IResourceTypeReader>()
                .InstancePerLifetimeScope();
        }
    }
}
