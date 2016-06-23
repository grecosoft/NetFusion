using NetFusion.Settings;

namespace RefArch.Domain.Samples.Settings
{
    public class MachineFileInitializedSettings : AppSettings
    {
        public int MinValue { get; set; } = 5;
        public int MaxValue { get; set; } = 6;
        public int[] ValidValues { get; set; } = new[] { 3, 8, 4, 1 };
    }
}
