using NetFusion.Settings;
using NetFusion.Settings.Strategies;

namespace NetFusion.Core.Tests.Settings.Mocks
{
    /// <summary>
    /// Mock open-generic initializer that can be used to load
    /// multiple types of application settings.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings.</typeparam>
    public class MockGenericIntTwo<TSettings> : AppSettingsInitializer<TSettings>
        where TSettings : IAppSettings
    {
        public bool SetValues { get; set; }

        protected override IAppSettings OnConfigure(TSettings settings)
        {
            var mockSettings = settings as MockSettings;
            if (mockSettings != null)
            {
                mockSettings.SettingValue = this.GetType().Name;
            }
            return settings;
        }
    }
}
