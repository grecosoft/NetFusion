using NetFusion.Settings;
using NetFusion.Settings.Strategies;

namespace NetFusion.Core.Tests.Settings.Mocks
{
    public class MockSettingInitOne : AppSettingsInitializer<MockSettings>
    {
        protected override IAppSettings OnConfigure(MockSettings settings)
        {
            settings.SettingValue = nameof(MockSettingInitOne);
            return settings;
        }
    }
}
