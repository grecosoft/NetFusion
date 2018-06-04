using NetFusion.Test.Plugins;
using System;
using NetFusion.Messaging;

namespace CoreTests.Queries
{
    public static class TestFixtureExtensions
    {
        public static TestTypeResolver WithDispatchConfiguredHost(this TestTypeResolver resolver, 
            params Type[] pluginTypes)
        {
            // Configure Core Plugin with messaging and the 
            // unit -of-work module.
            resolver.AddPlugin<MockCorePlugin>()
                 .UseMessagingPlugin();

            // Add host plugin with the plugin-types to be used
            // for the unit-test.
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType(pluginTypes);

            return resolver;
        }
    }
}
