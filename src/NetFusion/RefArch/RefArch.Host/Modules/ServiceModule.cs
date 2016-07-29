using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Scripting;
using NetFusion.Integration.Domain.Evaluation;

namespace RefArch.Host.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<EntityExpresionSetRepository>()
                .As<IEntityScriptRepository>()
                .InstancePerLifetimeScope();
        }
    }
}