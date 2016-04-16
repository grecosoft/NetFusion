using NetFusion.Bootstrap.Testing;
using NetFusion.Core.Tests.Bootstrap.Mocks;

namespace NetFusion.Tests.Bootstrap
{
    public static class BootstrapTestExtensions
    {
        public static void SetupHostApp(this HostTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>();
        }
    }
}
