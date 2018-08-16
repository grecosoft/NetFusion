using System.Linq;
using Demo.Infra;

namespace Demo.App
{
    public class MinCalculator : IValueCalculator
    {
        public int Sequence => 1;

        public int GetValue(int[] values)
        {
            return values.Min();
        }
    }
}
