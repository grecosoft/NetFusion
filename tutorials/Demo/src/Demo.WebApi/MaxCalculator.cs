using System.Linq;
using Demo.Core;

namespace Demo.WebApi
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
