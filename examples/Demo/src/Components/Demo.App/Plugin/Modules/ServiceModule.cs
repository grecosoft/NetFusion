using Demo.App.Services;
using Demo.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Plugin.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void ScanPlugins(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterface("Service", ServiceLifetime.Scoped);
        }
        
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<SampleEntityService>();
            services.AddScoped<IEntityIdGenerator, EntityIdGenerator>();
        }
    }
}
