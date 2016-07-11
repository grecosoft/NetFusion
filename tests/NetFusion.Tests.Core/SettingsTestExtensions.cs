using NetFusion.Bootstrap.Testing;
using NetFusion.Settings.Testing;

namespace NetFusion.Tests.Core.Settings
{
    public static class SettingsTestExtensions
    {
        public static void SetupSettingsPlugin(this TestTypeResolver resolver)
        {
            resolver.AddSettingsPlugin();
        }
    }
}
