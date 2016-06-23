using NetFusion.Settings;
using NetFusion.Settings.Strategies;
using RefArch.Domain.Samples.Settings;

namespace RefArch.Infrastructure.Samples.Settings
{
    public class TestInitializer : AppSettingsInitializer<InitializedSettings>
    {
        protected override IAppSettings OnConfigure(InitializedSettings settings)
        {
            settings.Value1 *= 2;
            settings.Value2 *= 2;

            // Initialize the values as above or return a new initialized
            // instance of the class.  If the initializer cannot satisfy the
            // request, it should return null.

            return settings;
        }
    }
}
