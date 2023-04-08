using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Entity;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.Server.Concurrency;

namespace NetFusion.Web.Rest.Server.Plugin.Modules;

public class ConcurrencyModule : PluginModule
{
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<EntityContext>();
            
        services.AddControllers(options =>
        {
            options.Filters.Add<ConcurrencyFilter>();
        });
    }
}