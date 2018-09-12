using System;
using NetFusion.Bootstrap.Container;

namespace Demo.Infra
{
    public class ValidRangeConfig : IContainerConfig
    {
        public int MinValue { get; private set; } = 5;
        public int MaxValue { get; private set; } = 10;

        public void SetRange(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new InvalidOperationException(
                    "MinValue must be less than or equal MaxValue.");
            }

            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
