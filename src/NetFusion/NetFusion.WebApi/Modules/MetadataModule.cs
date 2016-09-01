using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.WebApi.Core;
using NetFusion.WebApi.Metadata;

namespace NetFusion.WebApi.Modules
{
    public class MetadataModule : PluginModule
    {
        public override void RegisterDefaultComponents(ContainerBuilder builder)
        {
            builder.RegisterType<RouteMetadataService>()
                .As<IRouteMetadataService>()
                .SingleInstance();
        }
    }
}
