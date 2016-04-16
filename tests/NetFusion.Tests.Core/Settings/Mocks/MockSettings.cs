using NetFusion.Settings;

namespace NetFusion.Core.Tests.Settings.Mocks
{
    /// <summary>
    /// Mock application settings class requiring at least one
    /// initializer to be configured.
    /// </summary>
    public class MockSettings : AppSettings
    {
        public string SettingValue { get; set; }
    }
}
