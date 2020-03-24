using System.Linq;
using Demo.Core;
using Demo.Core.Plugin;

namespace Demo.App
{
    public class CalculateService : ICalculateService
    {
        private readonly ICalculatorModule _calculatorModule;

        public CalculateService(ICalculatorModule calculatorModule)
        {
            _calculatorModule = calculatorModule;
        }

        public int CalculateValue(int[] values)
        {
            int value = 0;
            foreach(var calculator in _calculatorModule.Calculators
                .OrderBy(c => c.Sequence).ToArray())
            {
                var currValue =  calculator.GetValue(values);
                if (currValue <= 7) continue;
                value += currValue;
            }

            return value;
        }
    }
}
