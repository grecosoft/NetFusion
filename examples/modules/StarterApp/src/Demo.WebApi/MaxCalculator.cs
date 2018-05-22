using System.Linq;
using Demo.Infra;

namespace Demo.App 
{
    public class MaxCalculator : IValueCalculator
    {
        public int Sequence => 0;

        public int GetValue(int[] values)
        {
            return values.Max();
        }
    }
}