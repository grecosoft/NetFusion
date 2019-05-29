using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using Service.App.Services;
using Service.Domain.Services;

namespace Service.App.Plugin.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IExampleResultLog, ExampleResultLog>();
        }
    }
}