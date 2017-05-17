using NetFusion.Settings;
using NetFusion.Test.Plugins;

namespace CoreTests.Settings
{
    public static class SettingsTestExtensions
    {
        public static TestTypeResolver DefaultSettingsConfig(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>();

            resolver.AddPlugin<MockCorePlugin>()
                 .UseSettingsPlugin();

            return resolver;
        }
    }
}
