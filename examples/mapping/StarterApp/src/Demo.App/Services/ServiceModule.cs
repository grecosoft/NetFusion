using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Services
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<SampleEntityService>();
            services.AddScoped<IEntityIdGenerator, EntityIdGenerator>();
        }
    }
}
