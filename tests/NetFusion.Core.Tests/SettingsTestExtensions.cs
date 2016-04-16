using NetFusion.Bootstrap.Testing;
using NetFusion.Settings.Testing;

namespace NetFusion.Tests.Core.Settings
{
    public static class SettingsTestExtensions
    {
        public static void SetupSettingsPlugin(this HostTypeResolver resolver)
        {
            resolver.AddSettingsPlugin();
        }
    }
}
