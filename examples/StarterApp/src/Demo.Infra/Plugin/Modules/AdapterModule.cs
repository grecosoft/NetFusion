using Demo.Domain.Adapters;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Infra.Plugin.Modules
{
    public class AdapterModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IRegistrationDataAdapter, RegistrationDataAdapter>();
            services.AddSingleton<ISalesDataAdapter, SalesDataAdapter>();
        }
    }
}
