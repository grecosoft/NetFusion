using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;

namespace Service.Infra.Plugin.Modules
{
    // Convention based registration.
    public class RepositoryModule : PluginModule
    {
        public override void ScanPlugins(ITypeCatalog catalog)
        { 
            catalog.AsImplementedInterface("Repository", ServiceLifetime.Scoped);
        }
    }
}