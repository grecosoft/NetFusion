using NetFusion.Settings;

namespace NetFusion.Core.Tests.Settings.Mocks
{
    /// <summary>
    /// Mock application settings class for which setting 
    /// initializers must not be configured.
    /// </summary>
    public class MockOptionalSettings : AppSettings
    {
        public MockOptionalSettings()
        {
            this.IsInitializationRequired = false;
        }

        public string DefaultValue1 { get; set; } = "Value1";
    }
}
