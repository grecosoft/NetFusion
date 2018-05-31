using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;

namespace Solution.Context.Infra.Repositories
{
    // Convention based registration.
    public class RepositoryModule : PluginModule
    {
        public override void ScanPlugin(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterfaces("Repository", ServiceLifetime.Scoped);
        }
    }
}