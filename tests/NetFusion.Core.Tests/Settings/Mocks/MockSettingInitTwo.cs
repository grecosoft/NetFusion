using NetFusion.Settings;
using NetFusion.Settings.Strategies;

namespace NetFusion.Core.Tests.Settings.Mocks
{
    public class MockSettingInitTwo : AppSettingsInitializer<MockSettings>
    {
        protected override IAppSettings OnConfigure(MockSettings settings)
        {
            settings.SettingValue = nameof(MockSettingInitTwo);
            return settings;
        }
    }
}
