using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Infra
{
    public interface ICalculatorModule : IPluginModuleService
    {
        IEnumerable<IValueCalculator> Calculators { get; }
        int CalculateValue(int[] values);
    }
}