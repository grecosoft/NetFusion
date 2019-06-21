using Demo.App.Services;
using Demo.Core;
using Demo.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Plugin.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ICalculateService, CalculateService>();
            services.AddScoped<SampleEntityService>();
            services.AddScoped<IEntityIdGenerator, EntityIdGenerator>();
        }
    }
}
