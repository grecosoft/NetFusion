using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.WebApi.Modules
{
    public interface IClaimTokenModule : IPluginModuleService
    {
        Type ApplicationPrincipalType { get; }
    }
}
