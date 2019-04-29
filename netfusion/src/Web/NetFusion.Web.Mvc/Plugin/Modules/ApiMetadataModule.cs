using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Metadata;
using NetFusion.Web.Mvc.Metadata.Core;
using NetFusion.Web.Mvc.Plugin.Configs;

namespace NetFusion.Web.Mvc.Plugin.Modules
{
    /// <summary>
    /// Plugin module configuring MVC to allow access to route 
    /// metadata at specified endpoints.
    /// </summary>
    public class ApiMetadataModule : PluginModule
    {
        private WebMvcConfig _mvcConfig;
      
        public override void Initialize()
        {
            _mvcConfig = Context.Plugin.GetConfig<WebMvcConfig>();
            _mvcConfig?.Validate();
        }

        // Determine if the host application specified the WebMvcConfig configuration
        // indicating that route metadata should be discoverable by clients.
        public override void Configure()
        {
            if (_mvcConfig != null && _mvcConfig.EnableRouteMetadata)
            {
                // Add convention used to determine which route methods
                // are to be contained within the returned metadata.
                _mvcConfig.Services.AddMvc((options) =>
                {
                    options.Conventions.Add(new ApiExplorerConvention());
                });
            }
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IApiMetadataService, ApiMetadataService>();
        }
    }
}
