using System.Linq;

namespace Demo.Infra
{
    public class DefaultCalculateService : ICalculateService
    {
        private readonly ICalculatorModule _calculatorModule;

        public DefaultCalculateService(ICalculatorModule calculatorModule)
        {
            _calculatorModule = calculatorModule;
        }

        public int CalculateValue(int[] values)
        {
            int value = 0;
            foreach(var calculator in _calculatorModule.Calculators.
                OrderBy(c => c.Sequence).ToArray())
            {
                value += calculator.GetValue(values);
            }

            return value;
        }
    }
}