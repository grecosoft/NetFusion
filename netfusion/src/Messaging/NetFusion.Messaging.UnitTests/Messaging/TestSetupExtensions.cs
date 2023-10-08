using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Messaging.UnitTests.Messaging;

public static class TestSetupExtensions
{
    // Adds a host plugin configured with the core messaging plugin.
    public static ICompositeContainer AddMessagingHost(this ICompositeContainer container)
    {
        container.AppBuilder.ServiceCollection.AddSingleton<IMockTestLog, MockTestLog>();
            
        var hostPlugin = new MockHostPlugin();
            
        container.RegisterPlugins(hostPlugin);
        container.RegisterPlugin<MessagingPlugin>();
            
        return container;
    }
}