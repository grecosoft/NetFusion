using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.ReferenceTypes.Core;

namespace NetFusion.Domain.MongoDB.ReferenceTypes
{
    public class ReferenceTypeModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<ReferenceTypeRepository>()
                .As<IReferenceTypeRepository>()
                .InstancePerLifetimeScope();
        }
    }
}
