using NetFusion.Settings;

namespace RefArch.Domain.Samples.Settings
{
    public class UninitializedSettings : AppSettings
    {
        public UninitializedSettings()
        {
            // This indicates that the settings can be used with the
            // values initialized in code can don't have to have a
            // corresponding IAppSettingsInitializer defined.
            this.IsInitializationRequired = false;
        }

        public int Value1 { get; set; } = 100;
        public int Value2 { get; set; } = 200;
    }
}

