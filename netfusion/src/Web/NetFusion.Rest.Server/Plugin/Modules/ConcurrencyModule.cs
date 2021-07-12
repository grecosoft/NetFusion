using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Entity;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Concurrency;

namespace NetFusion.Rest.Server.Plugin.Modules
{
    public class ConcurrencyModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<EntityContext>();
            
            services.AddControllers(options =>
            {
                options.Filters.Add<ConcurrencyFilter>();
            });
        }
    }
}