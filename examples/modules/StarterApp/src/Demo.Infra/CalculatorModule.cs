using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Infra
{
    public class CalculatorModule : PluginModule,
        ICalculatorModule
    {
        public IEnumerable<IValueCalculator> Calculators { get; set; }

	public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ICalculateService, DefaultCalculateService>();
        }

        public int CalculateValue(int[] values)
        {
            int value = 0;
            foreach(var calculator in Calculators.OrderBy(c => c.Sequence).ToArray())
            {
                value += calculator.GetValue(values);
            }

            return value;
        }
    }
}
