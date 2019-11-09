using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;

namespace Demo.Core.Plugin.Modules
{
    public class NumberGeneratorModule : PluginModule
    {
        public override void ScanPlugins(ITypeCatalog catalog)
        {
            catalog.AsService<INumberGenerator>(
                t => t.IsConcreteTypeDerivedFrom<INumberGenerator>(),
                ServiceLifetime.Singleton);

        }
    }
}
