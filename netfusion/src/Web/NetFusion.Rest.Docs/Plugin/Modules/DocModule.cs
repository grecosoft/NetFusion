using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
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


        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IXmlCommentService, XmlCommentService>();
            services.AddSingleton<ITypeCommentService, XmlTypeCommentService>();

            services.AddScoped<IApiDocService, ApiDocService>();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            // Registers the configured document description implementations
            // with the dependency-injection container.
            foreach (Type descriptionType in RestDocConfig.DescriptionTypes)
            {
                services.AddScoped(typeof(IDocDescription), descriptionType);
            }

            services.AddControllers(config =>
            {
            
            });
        }
    }
}