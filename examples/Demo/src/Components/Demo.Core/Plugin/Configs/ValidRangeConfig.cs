using System;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Core.Plugin.Configs
{
    public class ValidRangeConfig : IPluginConfig
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
