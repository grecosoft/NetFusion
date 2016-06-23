using NetFusion.Settings;

namespace RefArch.Domain.Samples.Settings
{
    public class InitializedSettings : AppSettings
    {
        public int Value1 { get; set; } = 500;
        public int Value2 { get; set; } = 700;
    }
}
