using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Infra.Repositories
{
    public class ServiceModule : PluginModule
    {
        public override void ScanPlugin(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterfaces("Repository", ServiceLifetime.Scoped);
        }
    }
}
