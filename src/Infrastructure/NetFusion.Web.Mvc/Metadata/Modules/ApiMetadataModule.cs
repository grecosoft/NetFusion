
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Metadata.Core;

namespace NetFusion.Web.Mvc.Metadata.Modules
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
            if (_mvcConfig != null)
            {
                _mvcConfig.Validate();
            }
        }

        // Determine if the host application specifed the WebMvcConfig configuration
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

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<ApiMetadataService>()
                .As<IApiMetadataService>()
                .SingleInstance();
        }
    }
}
