using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Scripting;

namespace RefArch.Subscriber
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<NullEntityScriptingService>()
                .As<IEntityScriptingService>()
                .SingleInstance();
        }
    }
}