using Autofac;
using NetFusion.Bootstrap.Plugins;
using RefArch.Domain.Samples.WebApi;
using RefArch.Services.WebApi;

namespace RefArch.Services.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<PrincipalDependentService>()
                .As<IPrincipalDependentService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<UserService>()
                .As<IUserService>()
                .InstancePerLifetimeScope();
        }
    }
}
