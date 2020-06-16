using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core;
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
        
        public void ApplyDescriptions<T>(Action<T> description)
            where T : IDocDescription
        {
            foreach(T instance in DescriptionConfig.Descriptions.OfType<T>())
            {
                description.Invoke(instance);
            }
        }

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