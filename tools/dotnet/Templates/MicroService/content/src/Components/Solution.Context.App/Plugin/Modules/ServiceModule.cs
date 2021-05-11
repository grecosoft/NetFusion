using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;

namespace Solution.Context.App.Plugin.Modules
{
    public class ServiceModule : PluginModule
    {
        public override void ScanForServices(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterface("Service", ServiceLifetime.Scoped);
        }
    }
}