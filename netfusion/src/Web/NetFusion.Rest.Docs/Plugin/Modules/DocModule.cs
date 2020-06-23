using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Plugin.Configs;
using NetFusion.Rest.Docs.XmlDescriptions;

namespace NetFusion.Rest.Docs.Plugin.Modules
{
    public class DocModule : PluginModule,
        IDocModule
    {
        public RestDocConfig RestDocConfig { get; private set; }
        
        public override void Initialize()
        {
            RestDocConfig = Context.Plugin.GetConfig<RestDocConfig>();
        }

        public IEnumerable<IDocDescription> GetDocDescriptions()
        {
            return RestDocConfig.DescriptionTypes.CreateInstancesDerivingFrom<IDocDescription>();
        }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IApiDocService, ApiDocService>();
            services.AddScoped<ITypeCommentService, XmlTypeCommentService>();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddControllers(config =>
            {
            
            });
        }
    }
}