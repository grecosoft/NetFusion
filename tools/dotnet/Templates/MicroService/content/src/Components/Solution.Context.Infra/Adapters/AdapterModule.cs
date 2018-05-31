using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;

namespace Solution.Context.Infra.Adapters
{
    // Convention based registration.
    public class AdapterModule : PluginModule
    {
        public override void ScanPlugin(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterfaces("Adapter", ServiceLifetime.Scoped);
        }
    }
}