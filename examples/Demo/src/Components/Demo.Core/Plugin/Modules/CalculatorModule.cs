using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.Plugin.Modules
{
    public class CalculatorModule : PluginModule, ICalculatorModule
    {
        public IEnumerable<IValueCalculator> Calculators { get; private set; }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ICalculateService, DefaultCalculateService>();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["calculators"] = Calculators.Select(c => new { calcType = c.GetType().FullName});
        }
    }
}