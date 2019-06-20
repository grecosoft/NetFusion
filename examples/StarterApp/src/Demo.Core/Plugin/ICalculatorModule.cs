using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Core
{
    public interface ICalculatorModule : IPluginModuleService
    {
        IEnumerable<IValueCalculator> Calculators { get; }
//        int CalculateValue(int[] values);
    }
}
