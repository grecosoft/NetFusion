using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Messaging.UnitTests;

public static class TestSetup
{
    public static void WithEventHandler(CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddModule<InterceptionTests.ExampleModule>();

        container.RegisterPlugin<MockHostPlugin>();
        container.RegisterPlugins(appPlugin);
        container.RegisterPlugin<MessagingPlugin>();
    }
}