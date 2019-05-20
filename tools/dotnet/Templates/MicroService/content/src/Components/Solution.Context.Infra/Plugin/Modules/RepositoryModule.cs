using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;

namespace Solution.Context.Infra.Repositories
{
    public class RepositoryModule : PluginModule
    {
        public override void ScanPlugins(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterface("Repository", ServiceLifetime.Scoped);
        }
    }
}