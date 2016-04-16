using System;
using Autofac;
using System.Linq;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Common;
using NetFusion.WebApi.Configs;

namespace NetFusion.WebApi.Modules
{
    /// <summary>
    /// Plug-in module that configures the needed known types specific to JWT security.
    /// </summary>
    public class ClaimTokenModule : PluginModule,
        IClaimTokenModule
    {
        private GeneralWebApiConfig GeneralConfig { get; set; }
        private Type _appPrincipalType;

        // IPluginModuleService
        public Type ApplicationPrincipalType { get { return _appPrincipalType; } }

        public override void Initialize()
        {
            GeneralConfig = Context.Plugin.GetConfig<GeneralWebApiConfig>();

            if (this.GeneralConfig.UseJwtSecurityToken)
            {
                _appPrincipalType = GetApplicationPrincipalType();
            }
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<JwtTokenService>()
                .As<IJwtTokenService>()
                .InstancePerLifetimeScope();
        }

        // Search for the derived FusionPrincipal application host type.  This is used
        // so an instance can be created dynamically at runtime.
        private Type GetApplicationPrincipalType()
        {
            var allAppHostTypes = this.Context.GetPluginTypesFrom(
                PluginTypes.AppHostPlugin,
                PluginTypes.AppComponentPlugin);

            var appPrincipalTypes = allAppHostTypes.Where(pt => pt.IsDerivedFrom<FusionPrincipal>());

            if (appPrincipalTypes.Empty())
            {
                throw new ContainerException(
                    $"An application principal deriving from {nameof(FusionPrincipal)} could not be " +
                    $"found within the application plug-ins.");
            }

            if (!appPrincipalTypes.IsSingletonSet())
            {
                throw new ContainerException(
                    $"More than one application principal deriving from {nameof(FusionPrincipal)} was found " +
                    $"within the application plug-ins.");
            }

            return appPrincipalTypes.First();
        }
    }
}
