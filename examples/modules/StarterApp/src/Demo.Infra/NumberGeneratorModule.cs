using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;

namespace Demo.Infra
{
    public class NumberGeneratorModule : PluginModule
    {
        public override void ScanAllOtherPlugins(ITypeCatalog catalog)
        {
            catalog.AsService<INumberGenerator>(
                t => t.IsConcreteTypeDerivedFrom<INumberGenerator>(), 
                ServiceLifetime.Singleton);
        }
    }
}