using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Metadata;
using NetFusion.Web.Mvc.Metadata.Core;

namespace NetFusion.Web.Mvc.Plugin.Modules
{
    /// <summary>
    /// Plugin module configuring MVC to allow access to route 
    /// metadata at specified endpoints.
    /// </summary>
    public class ApiMetadataModule : PluginModule
    {
        // Determine if the host application specified the WebMvcConfig configuration
        // indicating that route metadata should be discoverable by clients.
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
