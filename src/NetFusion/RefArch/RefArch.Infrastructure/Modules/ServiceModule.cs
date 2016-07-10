using Autofac;
using NetFusion.Bootstrap.Plugins;
using RefArch.Domain.Samples.MongoDb;
using RefArch.Infrastructure.Samples.MongoDB;
using RefArch.Infrastructure.Samples.Settings;

namespace RefArch.Infrastructure.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsInitService>()
                .As<ISettingsInitService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ExampleRepository>()
                .As<IExampleRepository>()
                .InstancePerLifetimeScope();
        }
    }
}
