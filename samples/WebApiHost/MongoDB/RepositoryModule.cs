using Autofac;
using NetFusion.Bootstrap.Plugins;
using WebApiHost.MongoDB.Repositories;

namespace WebApiHost.MongoDB
{
    public class RepositoryModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<CustomerRepository>()
                .As<ICustomerRepository>()
                .InstancePerLifetimeScope();
        }
    }
}
