using Autofac;
using NetFusion.Bootstrap.Plugins;
using WebApiHost.EntityFramework;
using WebApiHost.EntityFramework.Contacts;

namespace WebApiHost.EntityFramwork
{
    public class RepositoryModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<AddressRepository>()
                .As<IAddressRepository>()
                .InstancePerLifetimeScope();
        }
    }
}
