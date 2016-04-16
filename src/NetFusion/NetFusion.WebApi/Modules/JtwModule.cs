using System.Web;
using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.WebApi.Configs;
using NetFusion.WebApi.Handlers;

namespace NetFusion.WebApi.Modules
{
    public class JtwModule : PluginModule
    {
        private GeneralWebApiConfig GeneralConfig { get; set; }

        public override void Initialize()
        {
            GeneralConfig = Context.Plugin.GetConfig<GeneralWebApiConfig>();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            if (GeneralConfig.UseJwtSecurityToken)
            {
                builder.RegisterType<JwtTokenValidationHandler>()
                    .AsSelf()
                    .InstancePerDependency();

                builder.Register(c => HttpContext.Current.User)
                    .AsSelf()
                    .InstancePerDependency();
            }
        }
    }
} 
