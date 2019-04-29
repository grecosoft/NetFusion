using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;
using Service.Domain.Repositories;
using Service.Infra.Repositories;

namespace Service.Infra.Plugin
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.modules#bootstrapping---modules

    // Convention based registration.
    public class RepositoryModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ICustomerRepository, CustomerRepository>();
        }

        public override void ScanPlugin(ITypeCatalog catalog)
        {
           // catalog.AsImplementedInterface("Repository", ServiceLifetime.Scoped);
        }
    }
}