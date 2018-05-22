using Demo.Infra;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App
{
    public class ServiceModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
           services.AddSingleton<ICalculateService, CalculateService>();
        }
    }
}