using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Plugin.Configs;

namespace NetFusion.Rest.Docs.Plugin.Modules
{
    public class DocModule : PluginModule,
        IDocModule
    {
        private DocDescriptionConfig DescriptionConfig { get; set; }
        
        public override void Initialize()
        {
            DescriptionConfig = Context.Plugin.GetConfig<DocDescriptionConfig>();
        }

        public IEnumerable<IDocDescription> GetDocDescriptions()
        {
            return DescriptionConfig.DescriptionTypes.CreateInstancesDerivingFrom<IDocDescription>();
        }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IApiDocService, ApiDocService>();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddControllers(config =>
            {
            
            });
        }
    }
}