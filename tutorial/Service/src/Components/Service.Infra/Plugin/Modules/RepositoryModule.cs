using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;

namespace Service.Infra.Plugin.Modules
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.modules#bootstrapping---modules

    // Convention based registration.
    public class RepositoryModule : PluginModule
    {
        public override void ScanPlugins(ITypeCatalog catalog)
        {
           catalog.AsImplementedInterface("Repository", ServiceLifetime.Scoped);
        }
    }
}