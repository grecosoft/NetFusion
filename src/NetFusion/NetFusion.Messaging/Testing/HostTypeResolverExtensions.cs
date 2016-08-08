using NetFusion.Bootstrap.Testing;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Modules;

namespace NetFusion.Messaging.Testing
{
    public static class HostTypeResolverExtensions
    {
        public static void AddMessagingPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MessagingModule>()
                .AddPluginType<MessagingConfig>();
        }
    }
}
