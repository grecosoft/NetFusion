using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;

namespace TestClient.Infra.Repositories
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.modules#bootstrapping---modules

    // Convention based registration.
    public class RepositoryModule : PluginModule
    {
        public override void ScanPlugin(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterfaces("Repository", ServiceLifetime.Scoped);
        }
    }
}