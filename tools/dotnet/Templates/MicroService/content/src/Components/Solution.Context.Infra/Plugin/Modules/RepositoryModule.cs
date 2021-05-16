using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;

namespace Solution.Context.Infra.Plugin.Modules
{
    public class RepositoryModule : PluginModule
    {
        public override void ScanForServices(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterface("Repository", ServiceLifetime.Scoped);
        }
    }
}