using NetFusion.Bootstrap.Testing;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Modules;

namespace NetFusion.Messaging.Testing
{
    /// <summary>
    /// Adds a mock core plug-in with the needed plug-in types required to
    /// bootstrap the Messaging plug-in.
    /// </summary>
    public static class HostTypeResolverExtensions
    {
        /// <summary>
        /// Adds core plug-in with required Messaging types.
        /// </summary>
        /// <param name="resolver">The test type resolver</param>
        public static void AddMessagingPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MessagingModule>()
                .AddPluginType<MessagingConfig>();
        }
    }
}
