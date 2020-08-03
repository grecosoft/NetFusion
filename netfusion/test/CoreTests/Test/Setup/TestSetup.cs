using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Plugin;
using NetFusion.Test.Plugins;

namespace CoreTests.Test.Setup
{
    public static class TestSetup
    {
        public static void WithEventHandler(CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<InterceptionTests.AppMessageHandler>();
            appPlugin.AddModule<InterceptionTests.ExampleModule>();

            container.RegisterPlugin<MockHostPlugin>();
            container.RegisterPlugins(appPlugin);
            container.RegisterPlugin<MessagingPlugin>();
        }
    }
}