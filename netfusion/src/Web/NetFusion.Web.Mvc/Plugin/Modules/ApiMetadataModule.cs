using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Metadata;
using NetFusion.Web.Mvc.Metadata.Core;

namespace NetFusion.Web.Mvc.Plugin.Modules
{
    /// <summary>
    /// Plugin module that registers a service used to query ASP.NET controller action metadata.
    /// </summary>
    public class ApiMetadataModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IApiMetadataService, ApiMetadataService>();
        }
    }
}
