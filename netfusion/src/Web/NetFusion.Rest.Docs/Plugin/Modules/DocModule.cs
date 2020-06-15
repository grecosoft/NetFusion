using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core;

namespace NetFusion.Rest.Docs.Plugin.Modules
{
    public class DocModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IApiDocService, ApiDocService>();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddControllers(config =>
            {
                config.Filters.Add<ApiDocFilter>();
            });
        }
    }
}