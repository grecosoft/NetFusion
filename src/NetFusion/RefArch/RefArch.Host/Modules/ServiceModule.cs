using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Services;
using NetFusion.Integration.Domain;
using NetFusion.Integration.Domain.Evaluation;

namespace RefArch.Host.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<ExpressionMetadataRepository>()
                .As<IExpressionMetadataRepository>()
                .InstancePerLifetimeScope();
        }
    }
}