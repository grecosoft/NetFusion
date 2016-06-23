using NetFusion.Settings;

namespace RefArch.Domain.Samples.Settings
{
    public class FileInitializedSettings : AppSettings
    {
        public int MinValue { get; set; } = 100;
        public int MaxValue { get; set; } = 800;
        public int[] ValidValues { get; set; } = new[] { 105, 145, 254, 685 };
    }
}
