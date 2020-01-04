using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Core.Plugin
{
    public interface ICalculatorModule : IPluginModuleService
    {
        IEnumerable<IValueCalculator> Calculators { get; }
    }
}
